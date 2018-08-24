using System;
namespace TBSGameCore.TriggerSystem
{
    public interface ITriggerScope
    {
        TriggerVariableDefine getVariable(string name);
        TriggerVariableDefine[] getVariables();
        object getVariableValue(string name);
        TriggerActionDefine getAction(string name);
        TriggerActionDefine[] getActions();
        TriggerExprDefine getFunc(string name);
        TriggerExprDefine[] getFuncs(Type returnType);
    }
}