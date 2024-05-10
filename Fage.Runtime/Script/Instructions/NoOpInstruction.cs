using Fage.Script.Instruction;

namespace Fage.Runtime.Script.Instructions
{
	/// <summary>
	/// <see cref="Handle"/>方法不需要执行操作的指令。
	/// 此类型用于支撑基础结构，不应在代码中直接引用，特别是声明该类型的变量，或强制转换到这个类型。
	/// </summary>
	/// <devdoc>
	/// <remarks>
	/// <para>
	/// <see cref="Handle"/>已密封。
	/// 当一类指令需要执行操作时，请继承<see cref="ScriptInstruction"/>的或其它子类。
	/// </para>
	/// <para>
	/// 类注释中的摘要说明，是为了最大限度减少指令实现变化时，将基类修改为其它类型，
	/// 而引发的中断性变更。
	/// </para>
	/// </remarks>
	/// </devdoc>
	public class NoOpInstruction(ScriptInstructionOpcode opcode)
		: ScriptInstruction(opcode)
	{
		public sealed override void Handle(ScriptingEnvironment env)
		{
			/* no-op as you can see */
		}
	}
}