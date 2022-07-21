using System;

namespace Squax.Actions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ActionAttribute : Attribute
    {
        public string Path
        {
            get; set;
        }

        public ActionAttribute(string path)
        {
            Path = path;
        }
    }
}