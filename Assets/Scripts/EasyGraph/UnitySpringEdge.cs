using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyGraph
{
   /* public class UnitySpringEdge<TVertex> : IEdge<TVertex>
         where TVertex : class

    {
        #region Private fields
        private readonly TVertex _source;
        private readonly TVertex _target;
        private double _weight;


        #endregion

        #region Public fields
        public double MinLength { get; set; }
        public double MaxLength { get; set; }
        public double CurrentLength { get; set; }
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
        public UnitySpringEdge(TVertex source, TVertex target)
        {
            _source = source;
            _target = target;
            _weight = 1;
        }

        public UnitySpringEdge(TVertex source, TVertex target, double weight)
        {
            _source = source;
            _target = target;
            _weight = weight;
        }

        public UnitySpringEdge(TVertex source, TVertex target, double weight, double minLength, double maxLength)
        {
            _source = source;
            _target = target;
            _weight = weight;
            MinLength = minLength;
            MaxLength = maxLength;
        }

        #endregion

        #region IEdge interface fuctions
        public TVertex GetOtherVertex(TVertex testVertex)
        {
            if (testVertex != Source && testVertex != Target) return null;
            return testVertex == Source ? Target : Source;
        }

        public void SetWeight(double weight)
        {
            _weight = weight;
        }
        #endregion
    }*/
}
