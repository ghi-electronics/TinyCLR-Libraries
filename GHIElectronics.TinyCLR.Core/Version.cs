namespace System {
    // A Version object contains four hierarchical numeric components: major, minor,
    // revision and build.  Revision and build may be unspecified, which is represented
    // internally as a -1.  By definition, an unspecified component matches anything
    // (both unspecified and specified), and an unspecified component is "less than" any
    // specified component.

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public sealed class Version // : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        // AssemblyName depends on the order staying the same
        private int _Major;
        private int _Minor;
        private int _Build;// = -1;
        private int _Revision;// = -1;

        public Version(int major, int minor, int build, int revision) {
            if (major < 0 || minor < 0 || revision < 0 || build < 0)
                throw new ArgumentOutOfRangeException();

            this._Major = major;
            this._Minor = minor;
            this._Revision = revision;
            this._Build = build;
        }

        public Version(int major, int minor) {
            if (major < 0)
                throw new ArgumentOutOfRangeException();

            if (minor < 0)
                throw new ArgumentOutOfRangeException();

            this._Major = major;
            this._Minor = minor;

            // Other 2 initialize to -1 as it done on desktop and CE
            this._Build = -1;
            this._Revision = -1;
        }

        // Properties for setting and getting version numbers
        public int Major => this._Major;
        public int Minor => this._Minor;
        public int Revision => this._Revision;
        public int Build => this._Build;
        public override bool Equals(object obj) {
            if (((object)obj == null) ||
                (!(obj is Version)))
                return false;

            var v = (Version)obj;
            // check that major, minor, build & revision numbers match
            if ((this._Major != v._Major) ||
                (this._Minor != v._Minor) ||
                (this._Build != v._Build) ||
                (this._Revision != v._Revision))
                return false;

            return true;
        }

        public override string ToString() {
            var retStr = this._Major + "." + this._Minor;

            // Adds _Build and then _Revision if they are positive. They could be -1 in this case not added.
            if (this._Build >= 0) {
                retStr += "." + this._Build;
                if (this._Revision >= 0) {
                    retStr += "." + this._Revision;
                }
            }

            return retStr;
        }
    }
}


