namespace System.Reflection {
    public class ParameterInfo {
        protected Type ClassImpl;
        protected int PositionImpl;

        public virtual Type ParameterType => this.ClassImpl;
        public virtual int Position => this.PositionImpl;
    }
}
