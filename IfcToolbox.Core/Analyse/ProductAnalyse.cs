using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.IO.Xml.BsConf;

namespace IfcToolbox.Core.Analyse
{
    public class ProductAnalyse
    {
        /// <summary>
        /// Pop all building element in spatial elements for InsertCopy.
        /// If only bring spatial element, the hierarchy will missing and IfcProject will not copied over.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static HashSet<IIfcProduct> PrepareRequiredProducts(IModel model, IEnumerable<IPersistEntity> entities)
        {
            return PrepareRequiredProducts(model, entities.Select(e => e.EntityLabel));
        }
        public static HashSet<IIfcProduct> PrepareRequiredProducts(IModel model, IPersistEntity entity)
        {
            return PrepareRequiredProducts(model, new List<int> { entity.EntityLabel });
        }

        /// <summary>
        /// Used in SplitByProduct
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static HashSet<IIfcProduct> PrepareRequiredProducts(IModel model, IEnumerable<string> entities)
        {
            var intList = entities.Select(s => new { Success = int.TryParse(s, out var value), value })
                 .Where(pair => pair.Success).Select(pair => pair.value);
            return PrepareRequiredProducts(model, intList);
        }
        public static HashSet<IIfcProduct> PrepareRequiredProducts(IModel model, IEnumerable<int> entityLables)
        {
            var requiredProducts = new HashSet<IIfcProduct>();
            var products = model.Instances.OfType<IIfcProduct>().Where(x => entityLables.Contains(x.EntityLabel));
            if (!products.Any())
                return requiredProducts;
            foreach (var entity in products)
            {
                if (entity is IIfcSpatialStructureElement)
                    foreach (var product in GetIfcProductInHierarchy(entity))
                        requiredProducts.Add(product);
                else
                    requiredProducts.Add(entity);
            }
            return requiredProducts;
        }

        public static HashSet<IIfcProduct> PrepareRequiredProductsByGlobalIds(IModel model, IEnumerable<string> globalIds)
        {
            var requiredProducts = new HashSet<IIfcProduct>();

            // 1) Filter by GlobalId
            var products = model.Instances
                .OfType<IIfcProduct>()
                .Where(p => globalIds.Contains(p.GlobalId.ToString()));

            // 2) For each matched product, gather it + parents + children
            foreach (var product in products)
            {
                // a) Add the product and all its parents (storey -> building -> site -> project)
                //CollectProductAndItsParents(product, requiredProducts);

                if (product is IIfcSpatialStructureElement)
                    foreach (var product2 in GetIfcParentProductsInHierarchy(product))
                        requiredProducts.Add(product2);

                // b) If the product is a spatial element (like IfcSpace), 
                //    also gather contained elements and sub‐spaces
                //CollectChildrenIfSpatial(product, requiredProducts);
            }

            return requiredProducts;
        }

        #region Private methodes
        private static HashSet<IIfcProduct> GetIfcProductInHierarchy(IIfcObjectDefinition o, HashSet<IIfcProduct> relatedEntities = null)
        {
            if (relatedEntities == null)
                relatedEntities = new HashSet<IIfcProduct>();
            if (o == null)
                return relatedEntities;

            //only spatial elements can contain building elements
            if (o is IIfcSpatialStructureElement spatialElement && spatialElement != null)
            {
                //using IfcRelContainedInSpatialElement to get contained elements
                var containedElements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
                foreach (var element in containedElements)
                    relatedEntities.Add(element);
            }

            //using IfcRelAggregares to get spatial decomposition of spatial structure elements
            foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
                GetIfcProductInHierarchy(item, relatedEntities);

            return relatedEntities;
        }

        private static HashSet<IIfcProduct> GetIfcParentProductsInHierarchy(IIfcObjectDefinition o, HashSet<IIfcProduct> relatedEntities = null)
        {
            if (relatedEntities == null)
                relatedEntities = new HashSet<IIfcProduct>();
            if (o == null)
                return relatedEntities;

            //only spatial elements can contain building elements
            if (o is IIfcSpatialStructureElement spatialElement && spatialElement != null)
            {
                //using IfcRelContainedInSpatialElement to get contained elements
                var containedElements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
                foreach (var element in containedElements)
                    relatedEntities.Add(element);
            }

            //using IfcRelAggregares to get spatial decomposition of spatial structure elements
            foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
                GetIfcProductInHierarchy(item, relatedEntities);

            return relatedEntities;
        }

        private static HashSet<IIfcObjectDefinition> GetDecomposesObject(IIfcObjectDefinition o, HashSet<IIfcObjectDefinition> relatedEntities = null)
        {
            if (relatedEntities == null)
                relatedEntities = new HashSet<IIfcObjectDefinition>();

            if (o != null && o.Decomposes != null)
                foreach (var item in o.Decomposes.Select(r => r.RelatingObject))
                {
                    relatedEntities.Add(item);
                    GetDecomposesObject(item, relatedEntities);
                }
            return relatedEntities;
        }

        // =============================
        //  A) Collect product + parents
        // =============================
        private static void CollectProductAndItsParents(IIfcObjectDefinition obj, HashSet<IIfcProduct> collected)
        {
            if (obj is IIfcProduct prod)
                collected.Add(prod);

            if (obj.Decomposes == null)
                return;

            foreach (var rel in obj.Decomposes)
            {
                var parent = rel.RelatingObject;
                if (parent is IIfcProduct parentProd)
                    collected.Add(parentProd);

                // Recurse up again, no downward
                CollectProductAndItsParents(parent, collected);
            }
        }

        // ==============================
        //  B) Collect children if spatial
        // ==============================
        private static void CollectChildrenIfSpatial(IIfcProduct prod, HashSet<IIfcProduct> collected)
        {
            // Only do this if it's a spatial element (e.g. IfcSpace)
            if (!(prod is IIfcSpatialElement spatial))
                return;

            // 1) Collect contained elements
            var contained = spatial.ContainsElements
                .SelectMany(rel => rel.RelatedElements)
                .OfType<IIfcProduct>();

            foreach (var item in contained)
                collected.Add(item);

            // 2) Collect sub‐spaces (if you actually want them)
            //    This picks children that are spatial elements decomposed below "prod"
            foreach (var rel in spatial.IsDecomposedBy)
            {
                foreach (var childObj in rel.RelatedObjects)
                {
                    CollectProductAndItsParents(childObj, collected);
                    CollectChildrenIfSpatial(childObj as IIfcProduct, collected);
                }
            }
        }
        #endregion
    }
}
