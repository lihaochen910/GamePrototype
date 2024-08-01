using Bang.Components;


namespace Pixpil.Components;

/// <summary>
/// 这个组件标志这状态机中包含可以绘制的StateAction, 挂载此组件以调用<see cref="Pixpil.AI.HFSM.HFSMStateAction.OnMurderDraw"/>绘制方法
/// </summary>
public readonly struct HFSMAgentDrawComponent : IComponent;
