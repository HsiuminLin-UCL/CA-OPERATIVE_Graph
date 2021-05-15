using System;
using System.Collections.Generic;


namespace EasyGraph
{
    public class Edge<TVertex> : IEdge<TVertex>
         where TVertex : class

    {
        #region Private fields
        private readonly TVertex _source;
        private readonly TVertex _target;
        private double _weight;

        #endregion

        #region IEdge interface fields
        /// <summary>
        /// Gets the source vertex
        /// </summary>
        public TVertex Source => _source;
        /// <summary>
        /// Gets the target vertex
        /// </summary>
        public TVertex Target => _target;

        /// <summary>
        /// Get the weight of this edge
        /// </summary>
        public double Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                _weight = value;
            }
        }
        #endregion

        #region Constructor
        public Edge(TVertex source, TVertex target)
        {
            _source = source;
            _target = target;
            _weight = 0.1f;
        }

        public Edge(TVertex source, TVertex target,double weight)
        {
            _source = source;
            _target = target;
            _weight = weight;
        }

        #endregion

        #region IEdge interface fuctions
        public TVertex GetOtherVertex(TVertex testVertex)
        {
            if (testVertex != Source && testVertex != Target) return null;
            return testVertex == Source ? Target : Source;
        }

        #endregion
    }
}
