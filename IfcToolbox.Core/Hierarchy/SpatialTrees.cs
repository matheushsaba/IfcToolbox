using System.Collections.Generic;
using System.Linq;

namespace IfcToolbox.Core.Hierarchy
{
    /// <summary>
    /// Arborescences 
    /// Used for splitter UI load
    /// </summary>
    public class SpatialTrees
    {
        public List<HierarchyNode> FullNodes { get; set; } = new List<HierarchyNode>();
        public List<HierarchyNode> TypedNodes { get; set; } = new List<HierarchyNode>();
        public List<HierarchyNode> SiteNodes { get; set; } = new List<HierarchyNode>();
        public List<HierarchyNode> BuildingNodes { get; set; } = new List<HierarchyNode>();
        public List<HierarchyNode> LevelNodes { get; set; } = new List<HierarchyNode>();

    }

}
