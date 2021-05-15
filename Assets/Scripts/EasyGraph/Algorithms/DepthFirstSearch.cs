using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyGraph
{
    //https://www.youtube.com/watch?v=7fujbpJ0LB4&ab_channel=WilliamFiset
    public class DepthFirstSearch<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : class
    {
        private IGraph<TVertex, TEdge> _graph;
        private int _numberOfVertices;
        private bool[] _visited; 


        public DepthFirstSearch(IGraph<TVertex, TEdge> graph)
        {
            _graph = graph;
        }

        public void StartDepthFirstSearch(TVertex startVertex)
        {
            _numberOfVertices = _graph.GetAllVertices().Count;
            _visited = new bool[_numberOfVertices];
        }

        private void DepthFirstSearchStep()
        {

        }
    }
}