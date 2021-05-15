using System.Collections;
using System.Collections.Generic;

namespace EasyGraph
{
    public interface IGraph<TVertex, TEdge> 
        where TEdge : IEdge<TVertex>
        where TVertex : class
    {
        /// <summary>
        /// Gets a value indicating if the graph allows parallel edges
        /// </summary>
        public bool AllowParallelEdges { get; }

        public Dictionary<TVertex, List<TEdge>> VertexEdgeDict { get; }

        public List<TVertex> GetAllVertices();
                public List<TEdge> GetEdges();
        public List<TEdge> GetConnectedEdges(TVertex vertex);
    }
}
