using Xbim.Common;

namespace IfcToolbox.Core.Hierarchy
{
    public class TreesReader
    {
        public static SpatialTrees GetSpatialTrees(IModel model)
        {
            var result = new SpatialTrees();
            result.FullNodes.Add(HierarchyReader.GetFullHierarchy(model));
            result.TypedNodes.AddRange(HierarchyReader.GetTypedHierarchy(model).Children);
            result.SiteNodes.Add(HierarchyReader.GetSpatialHierarchy(model, "IfcSite"));
            result.BuildingNodes.Add(HierarchyReader.GetSpatialHierarchy(model, "IfcBuilding"));
            result.LevelNodes.Add(HierarchyReader.GetSpatialHierarchy(model, "IfcBuildingStorey"));
            return result;
        }

    }
}
