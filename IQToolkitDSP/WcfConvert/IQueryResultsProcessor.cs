
using System.Collections;
using System.Collections.Generic;

namespace IQToolkitDSP
{
    internal interface IQueryResultsProcessor
    {
        object ProcessSingleResult(object result, DSPContext context);
        IEnumerable ProcessResults(IEnumerable results, DSPContext context);
    }
}
