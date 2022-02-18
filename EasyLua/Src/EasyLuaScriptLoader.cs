using System.Collections;
using System.Collections.Generic;

namespace EasyLua {
    public abstract class EasyLuaScriptLoader : IEnumerable<string> {
        public IEnumerator<string> GetEnumerator() {
            while (true) {
                var val = GetNextScript();
                if (!string.IsNullOrWhiteSpace(val)) {
                    yield return val;
                } else {
                    break;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        protected abstract string GetNextScript();
    }
}