
using System;
using System.Reflection;
using Mono.Cecil.Cil;

namespace Thea2ScriptLoaderFix
{
    using System.Collections.Generic;
    using Mono.Cecil;

    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        // Adds a simple log to the method so it is no longer inlined and Harmony can patch it
        public static void Patch(ref AssemblyDefinition assembly)
        {
            MethodInfo writeLineMethod = typeof(Console).GetMethod("ResetColor", new Type[] { });
            foreach (Mono.Cecil.TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.FullName == "Thea2.Common.NetCard")
                {
                    foreach (Mono.Cecil.MethodDefinition method in type.Methods)
                    {
                        if (method != null && method.FullName == "System.Int32 Thea2.Common.NetCard::GetCastingCost()")
                        {
                            ILProcessor worker = method.Body.GetILProcessor();

                            //string sentence;
                            //sentence = String.Concat("Added a line to prevent method to be inlined. ", method.Name);

                            //Import the Console.WriteLine() method
                            MethodReference writeLine;
                            writeLine = assembly.MainModule.Import(writeLineMethod);

                            //Creates the MSIL instruction for inserting the sentence
                            //Instruction insertSentence;
                            //insertSentence = worker.Create(OpCodes.Ldstr, sentence);

                            //Creates the CIL instruction for calling the
                            //Console.WriteLine(string value) method
                            Instruction callWriteLine;
                            callWriteLine = worker.Create(OpCodes.Call, writeLine);

                            //Getting the first instruction of the current method
                            Instruction ins = method.Body.Instructions[0];

                            //Inserts the insertSentence instruction before the first //instruction
                            //method.Body.GetILProcessor().InsertBefore(ins, insertSentence);
                            method.Body.GetILProcessor().InsertBefore(ins, callWriteLine);

                            //Inserts the callWriteLineMethod after the //insertSentence instruction
                            //worker.InsertAfter(insertSentence, callWriteLine);
                        }
                    }
                }
            }
        }
    }
}
