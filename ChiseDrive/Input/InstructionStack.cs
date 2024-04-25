using System;
using System.Collections.Generic;
using System.Text;

namespace ChiseDrive.Input
{
    /// <summary>
    /// There could possibly be lots of commands issued per update.  Or just one.  Or none.
    /// Usage:
    /// 
    /// InstructionStack instructions = new InstructionStack();
    /// instructions.Stack += new Instruction(...);
    /// Instruction = instructions.Pop();
    /// </summary>
    sealed public class InstructionStack
    {
        const int MaxInstructions = 20;
        private Instruction[] stack = new Instruction[MaxInstructions];
        int allocatedInstructions = 0;

        public bool IsEmpty
        {
            get
            {
                return allocatedInstructions == 0 ? true : false;
            }
        }

        public Instruction Pop()
        {
            if (allocatedInstructions == 0) return Instruction.Empty;

            allocatedInstructions--;
            return stack[allocatedInstructions];
        }

        public void Clear()
        {
            allocatedInstructions = 0;
        }

        public InstructionStack() { }

        public void Add(Instruction add)
        {
            if (allocatedInstructions == stack.Length) return;

            stack[allocatedInstructions] = add;
            allocatedInstructions++;
        }

        public void Add(InstructionStack add)
        {
            for (int i = 0; i < add.allocatedInstructions; i++)
            {
                Add(add.stack[i]);
            }
        }
    }
}
