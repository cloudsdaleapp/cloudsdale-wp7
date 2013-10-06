namespace CloudsdaleWin7.lib.Models
{
    public interface IMergable
    {
        /// <summary>
        /// Mergest another model into this one
        /// </summary>
        /// <param name="other">The other model to merge</param>
        void Merge(CloudsdaleModel other);
        /// <summary>
        /// Determines if this model can be merged with another
        /// </summary>
        /// <param name="other">The other model attempting to merge into this one</param>
        /// <returns>Whether a merge operation can be completed</returns>
        bool CanMerge(CloudsdaleModel other);
    }
}