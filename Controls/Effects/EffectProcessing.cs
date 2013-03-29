using System.Collections.Generic;
using System.Windows.Documents;

namespace Cloudsdale.Controls.Effects {
    public delegate IEnumerable<TextGroup> EffectHandler(string input); 

    public class TextGroup {
        public string Text { get; set; }
        public Inline Inline { get; set; }
    }
}
