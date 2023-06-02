
namespace Chess.TreesOfAnalysis
{
    public class AnalysisStoppedException : ApplicationException
    {
        public AnalysisStoppedException()
        { }

        public AnalysisStoppedException(string message) : base(message)
        { }
    }
}
