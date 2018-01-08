using System;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public class UnmappedValueExceptionHandler
    {
        public static void ThrowException(string fieldToUpdateDescription, UnmappedValueException ex)
        {
            string fromSyncSideText;
            string toSyncSideText;

            if (ex.SyncDirection == SyncDirection.SourceToTarget)
            {
                fromSyncSideText = "Source";
                toSyncSideText = "Target";
            }
            else if (ex.SyncDirection == SyncDirection.TargetToSource)
            {
                fromSyncSideText = "Target";
                toSyncSideText = "Source";
            }
            else
                throw new EnumValueNotImplementedException<SyncDirection>(ex.SyncDirection);

            throw new Exception(string.Format("{0}-side field '{1}' could not be updated. The {2}-side value '{3}' does not have a corresponding mapped value on the {4}-side. Ensure the {4}-side system has the corresponding value (usually a dropdown option) and that the integration mapping for the field has been updated.",
                                               toSyncSideText, fieldToUpdateDescription, fromSyncSideText.ToLower(), 
                                               ex.ValueWithoutMap == null ? "NULL" : ex.ValueWithoutMap, toSyncSideText.ToLower()));
        }
    }
}
