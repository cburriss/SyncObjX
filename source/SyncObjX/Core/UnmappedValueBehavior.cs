
namespace SyncObjX.Core
{
    /// <summary>
    /// Specifies the behavior of a field map value that is not explicitly mapped.
    /// </summary>
    public enum UnmappedValueBehavior
    {
        /// <summary>
        /// Uses the default value.
        /// </summary>
        UseDefaultValue,

        /// <summary>
        /// The value is passed through as is.
        /// </summary>
        PassThrough,

        /// <summary>
        /// No changes are applied.
        /// </summary>
        DoNotUpdate,

        // TODO: wrap each row iteration in a try/catch for data maps; add new class "RecordWithErrors"; Add new JobTerminateOnErrorType; don't execute any child records under RecordWithErrors 
        /// <summary>
        /// Throws an exception if a value is not mapped.
        /// </summary>
        ThrowException
    }
}
