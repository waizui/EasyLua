using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;

namespace EasyLua.Lexer {
    public class EasyLuaSyntaxError : Exception {
        public EasyLuaSyntaxError(string msg) : base(msg) {
        }
    }

    public abstract class Token {
        protected string name;
        protected string value;
    }

    public class FieldToken : Token {
        public string GetFieldName() {
            return name;
        }

        public string GetFieldType() {
            return value;
        }

        public string GetModifier() {
            return mModifier;
        }

        private string mModifier;

        public FieldToken(string name, string value, string modifier) {
            this.name = name;
            this.value = value;
            this.mModifier = modifier;
        }
    }

    public class EasyLuaLexer {
        private const string CLASS_NAME_PAT = @"^\s*---\s*@class\s+(?<className>\w+)\s*(:\s*(?<baseName>\w+))?";
        private const string Field_NAME_PAT = @"^\s*---\s*@field\s+(?<modifier>\w+)\s+(?<fieldName>\w+)\s+(?<typeName>[A-Za-z0-9_\.\[\]]+)";
        private const string COMMENT_PAT = @"^\s*---\s*@.*";
        private static Regex sClsNameRx = new Regex(CLASS_NAME_PAT, RegexOptions.Compiled);
        private static Regex sFieldNameRx = new Regex(Field_NAME_PAT, RegexOptions.Compiled);
        private static Regex sCommentRx = new Regex(COMMENT_PAT, RegexOptions.Compiled);

        private string mScript;
        private string mClassName;

        public string GetLuaClassName() {
            return mClassName;
        }

        private string mBaseName;

        public string GetBaseClassName() {
            return mBaseName;
        }

        private List<FieldToken> mFields = null;

        public List<FieldToken> GetScriptFields() {
            if (mFields == null) {
                mFields = ReadFields();
            }

            return mFields;
        }

        public EasyLuaLexer(string script) {
            Assert.IsFalse(string.IsNullOrWhiteSpace(script));
            mScript = PrepareScript(script);
            CompileClass();
        }

        private string PrepareScript(string script) {
            var sr = new StringReader(script);
            string consumed = null;
            while (true) {
                var peek = sr.Peek();
                if (peek <= 0) {
                    break;
                }

                var line = sr.ReadLine();
                if (line == null) {
                    break;
                }

                if (!IsComment(line)) {
                    consumed = line;
                    break;
                }
            }

            return consumed + Environment.NewLine + sr.ReadToEnd();
        }

        private bool IsComment(string line) {
            return !sCommentRx.IsMatch(line);
        }

        public string GetScript() {
            return mScript;
        }

        private void CompileClass() {
            var reader = new StringReader(mScript);
            var classDef = reader.ReadLine();
            // 第一行要写类型申明
            ReadClass(classDef);
        }


        private void ReadClass(string line) {
            var match = sClsNameRx.Match(line);
            if (!match.Success) {
                throw new EasyLuaSyntaxError("class define syntax error 在第一行申明类型 file");
            }

            var groups = match.Groups;
            for (int i = 0; i < groups.Count; i++) {
                var g = groups[i];
                var name = g.Name;
                if (g.Captures.Count == 0) {
                    continue;
                }

                var val = g.Captures[0].Value;
                if (name == "className") {
                    mClassName = val;
                } else if (name == "baseName") {
                    mBaseName = val;
                }
            }
        }

        public List<FieldToken> ReadFields() {
            var reader = new StringReader(mScript);
            // 去掉第一行
            reader.ReadLine();
            var fields = new List<FieldToken>();
            while (true) {
                var line = reader.ReadLine();
                if (line == null) {
                    break;
                }

                var token = ParseField(line);
                if (token != null) {
                    if (token.GetModifier() == "public") {
                        fields.Add(token);
                    }
                } else {
                    break;
                }
            }

            return fields;
        }

        private FieldToken ParseField(string line) {
            var match = sFieldNameRx.Match(line);
            if (!match.Success) {
                return null;
            }

            var groups = match.Groups;
            string fieldType = null;
            string fieldName = null;
            string modifier = null;
            var found = false;
            for (int i = 0; i < groups.Count; i++) {
                var g = groups[i];
                var name = g.Name;
                if (g.Captures.Count == 0) {
                    continue;
                }

                var val = g.Captures[0].Value;
                if (name == "modifier" && !string.IsNullOrEmpty(val)) {
                    modifier = val;
                    found = true;
                } else if (name == "fieldName") {
                    fieldName = val;
                } else if (name == "typeName") {
                    fieldType = val;
                }
            }

            if (found && !string.IsNullOrWhiteSpace(fieldName) && !string.IsNullOrWhiteSpace(fieldType)) {
                return new FieldToken(fieldName, fieldType, modifier);
            }

            return null;
        }

        public static string TrimExtension(string fileName) {
            return Regex.Replace(fileName, @"(\.\w*)", "", RegexOptions.Compiled);
        }
    }
}