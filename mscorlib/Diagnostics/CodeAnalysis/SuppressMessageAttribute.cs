namespace System.Diagnostics.CodeAnalysis {

    [AttributeUsage(
     AttributeTargets.All,
     Inherited = false,
     AllowMultiple = true
     )
    ]
    [Conditional("CODE_ANALYSIS")]
    public sealed class SuppressMessageAttribute : Attribute {
        private string category;
        private string justification;
        private string checkId;
        private string scope;
        private string target;
        private string messageId;

        public SuppressMessageAttribute(string category, string checkId) {
            this.category = category;
            this.checkId = checkId;
        }

        public string Category => this.category;
        public string CheckId => this.checkId;
        public string Scope {
            get => this.scope; set => this.scope = value;
        }

        public string Target {
            get => this.target; set => this.target = value;
        }

        public string MessageId {
            get => this.messageId; set => this.messageId = value;
        }

        public string Justification {
            get => this.justification; set => this.justification = value;
        }
    }
}