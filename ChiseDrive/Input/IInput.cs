using System;
using System.Collections.Generic;
using System.Text;

namespace ChiseDrive.Input
{
    /// <summary>
    /// The interface that governs user inputs.
    /// 
    /// Usage:
    /// 
    /// IInput input = new IInputDerived();
    /// 
    /// Call once per update cycle:
    /// 
    /// input.OpenInput();
    /// InstructionStack instructions = input.ParseInput();
    /// input.CloseInput();
    /// 
    /// </summary>
    public interface IInput
    {
        InstructionStack GetInput();
    }
}
