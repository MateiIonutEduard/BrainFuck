using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Brainfuck
{
    unsafe class BufferStream
    {
        private byte* input;
        private byte* buffer;
        private List<byte> ol;
        private int bi, ii;
        private int length;

        public BufferStream(byte[] input)
        {
            ol = new List<byte>();
            byte[] buffer = new byte[32768];
            length = input.Length;

            fixed (void* ptr = input)
                this.input = (byte*)ptr;

            fixed (void* ptr = buffer)
                this.buffer = (byte*)ptr;
        }

        public static string TranslateCode(string code)
        {
            var dictionary = new Dictionary<char, string>();
            dictionary.Add('>', " ptr++;\n if (ptr >= 32768) ptr = 0;\n");
            dictionary.Add('<', " ptr--;\n if(ptr < 0) ptr = 32767;\n");
            dictionary.Add('+', " stack[ptr]++;\n");
            dictionary.Add('-', " stack[ptr]--;\n");
            dictionary.Add('.', " write(cast(char)stack[ptr]);\n");
            dictionary.Add(',', "\n if(mptr < memory.length) stack[ptr]=memory[mptr++];\n else stack[ptr] = 0;");
            dictionary.Add('[', "\n while(stack[ptr] != 0) {\n");
            dictionary.Add(']', "}\n");

            string newCode = " import std.stdio;\n import core.stdc.stdlib;\n ubyte[32768] stack;\n void main(string[] args) { \n int ptr = 0;\n int mptr = 0; \n if(args.length == 2) {\n" +
            " ubyte[] memory = cast(ubyte[])args[1];\n " +
            string.Join(string.Empty, code.Where(c => dictionary.ContainsKey(c)).Select(c => dictionary[c])) + "} else {\n writeln(\"Requires only two argument.\");\n exit(-1);\n}\n}";
            return newCode;
        }

        /// <summary>
        /// Execute the source code of Brainfuck compiler.
        /// </summary>
        /// <param name="code"></param>
        public void ExecuteCode(BackgroundWorker bw, string code)
        {
            int i = 0;
            int right = code.Length;
            
            while (i < right)
            {
                switch (code[i])
                {
                    case '>':
                        {
                            bi++;
                            if (bi >= 32768) bi = 0;
                            break;
                        }
                    case '<':
                        {
                            bi--;
                            if (bi < 0) bi = 32768;
                            break;
                        }
                    case '.':
                        {
                            ol.Add(buffer[bi]);
                            break;
                        }
                    case '+':
                        {
                            buffer[bi]++;
                            break;
                        }
                    case '-':
                        {
                            buffer[bi]--;
                            break;
                        }
                    case '[':
                        {
                            if (bw.CancellationPending) return;

                            if (buffer[bi] == 0)
                            {
                                int loop = 1;

                                while (loop > 0)
                                {
                                    i++;
                                    char c = code[i];
                                    if (c == '[') loop++;
                                    else if (c == ']') loop--;
                                }
                            }
                            break;
                        }
                    case ']':
                        {
                            if (bw.CancellationPending) return;
                            int loop = 1;

                            while (loop > 0)
                            {
                                i--;
                                char c = code[i];
                                if (c == '[') loop--;
                                else if(c == ']') loop++;
                            }
                            i--;
                            break;
                        }
                    case ',':
                        {
                            if (ii == length) buffer[bi] = 0;
                            else buffer[bi] = input[ii++];
                            break;
                        }
                }

                i++;
            }
        }

        /// <summary>
        /// Reset the stack of compiler.
        /// </summary>
        public void ResetCompiler()
        {
            for (int i = 0; i < 32768; i++)
                buffer[i] = 0;
        }

        /// <summary>
        /// Returns the output of the program.
        /// </summary>
        /// <returns></returns>
        public byte[] GetOutput()
        {
            byte[] msg = ol.ToArray();
            return msg;
        }
    }
}
