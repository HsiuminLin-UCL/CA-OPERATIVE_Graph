using System;
using System.Collections;
using System.Collections.Generic;

namespace EasyGraph
{
    public interface IEdge<TVertex>
        where TVertex : class
    {
        #region fields
        TVertex Source { get; }
        TVertex Target { get; }
        double Weight { get; }
        #endregion

        #region functions
        public TVertex GetOtherVertex(TVertex testVertex);
        #endregion
    }
}
