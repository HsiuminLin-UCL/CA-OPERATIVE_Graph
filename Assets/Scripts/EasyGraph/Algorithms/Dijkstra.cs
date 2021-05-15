using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyGraph
{
    public class Dijkstra<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : class
    {
        private Dictionary<TVertex, (double, TVertex)> _distances;
        private IGraph<TVertex, TEdge> _graph;


        public double MaxDistance => _distances.Values.Max(s => s.Item1);
        

        public double VertexWeight(TVertex vertex) => _distances[vertex].Item1;

        public Dijkstra(IGraph<TVertex, TEdge> graph)
        {
            _graph = graph;
        }

        public void DijkstraCalculateWeights(TVertex source)
        {
            InitializeDistances();
            List<TVertex> unvisitedVertices = _graph.GetAllVertices();
            List<TVertex> visitedVertices = new List<TVertex>();

            //Set the source distance to zero
            _distances[source] = (0, null);

            while (unvisitedVertices.Count > 0)
            {
                TVertex nextVertex;
                //find the next vertex to check
                if (visitedVertices.Count == 0)
                    nextVertex = source;
                else
                    nextVertex = GetNextVertex(visitedVertices, unvisitedVertices);

                visitedVertices.Add(nextVertex);
                unvisitedVertices.Remove(nextVertex);

                foreach (var edge in _graph.GetConnectedEdges(nextVertex))
                {
                    TVertex linkedVertex = edge.GetOtherVertex(nextVertex);
                    double newWeight = _distances[nextVertex].Item1 + edge.Weight;
                    //Check if the connected vertex is unvisited
                    //and the distance is smaller than the allready defined distsance
                    if (unvisitedVertices.Contains(linkedVertex)
                        && newWeight < _distances[linkedVertex].Item1)
                    {
                        _distances[linkedVertex] = (newWeight, nextVertex);
                    }
                }
            }
        }

        //Get the vertex with the minimum distance and its distance from the vertices not included in the shortest path tree
        private TVertex GetNextVertex(List<TVertex> visitedVertices, List<TVertex> unvisitedVertices)
        {
            // Initialize min value 
            double min = double.MaxValue;
            TVertex closestVertex = null;



            foreach (var vertex in visitedVertices)
            {
                List<TEdge> connectedEdges = _graph.GetConnectedEdges(vertex);

                foreach (var edge in connectedEdges)
                {
                    TVertex connectedVertex = edge.GetOtherVertex(vertex);
                    if (unvisitedVertices.Contains(connectedVertex) && _distances[vertex].Item1 + edge.Weight < min)
                    {
                        min = edge.Weight;
                        closestVertex = connectedVertex;
                    }
                }
            }

            return closestVertex;
        }

        public List<TVertex> GetShortestPath(TVertex source, TVertex target)
        {
            DijkstraCalculateWeights(source);
            Stack<TVertex> shortestPath = new Stack<TVertex>();
            shortestPath.Push(target);
            while (shortestPath.Peek() != source)
            {
                shortestPath.Push(_distances[shortestPath.Peek()].Item2);
            }
            return new List<TVertex>(shortestPath);
        }

        private void InitializeDistances()
        {
            _distances = new Dictionary<TVertex, (double, TVertex)>();
            foreach (var vertex in _graph.GetAllVertices())
            {
                _distances.Add(vertex, (double.MaxValue, null));
            }
        }
    }
}
