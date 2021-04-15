using System;
using System.CodeDom;
using Codo = BJSYGameCore.CodeDOMHelper;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Animations;
// ReSharper disable SuggestVarOrType_SimpleTypes
namespace BJSYGameCore.AutoCompo
{
    public abstract class ComponentGenerator
    {
        public abstract void onGen(AutoCompoGenerator generator, Component component, bool isRootComponent, AutoBindFieldInfo info, CodeMemberField field);
    }
    public abstract class ComponentGenerator<T> : ComponentGenerator where T : Component
    {
        public sealed override void onGen(AutoCompoGenerator generator, Component component, bool isRootComponent, AutoBindFieldInfo info, CodeMemberField field)
        {
            onGen(generator, component as T, isRootComponent, info, field);
        }
        public abstract void onGen(AutoCompoGenerator generator, T component, bool isRootComponent, AutoBindFieldInfo info, CodeMemberField field);
    }
    class ButtonGenerator : ComponentGenerator<Button>
    {
        public override void onGen(AutoCompoGenerator generator, Button component, bool isRootComponent, AutoBindFieldInfo info, CodeMemberField field)
        {
            string name = field.Name.Substring(2, field.Name.Length - 2);
            name = name.headToUpper();
            //事件
            CodeMemberEvent Event;
            if (isRootComponent)
                Event = generator.genEvent(typeof(Action).Name, "onClick", Codo.type(generator.type.Name));
            else
                Event = generator.genEvent(typeof(Action).Name, "on" + name + "Click", Codo.type(generator.type.Name));
            //回调函数
            CodeMemberMethod callbackMethod;
            if (isRootComponent)
            {
                callbackMethod = generator.genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), "clickCallback");
                callbackMethod.Statements.Add(new CodeConditionStatement(
                new CodeBinaryOperatorExpression(Codo.getEvent(Event.Name), CodeBinaryOperatorType.IdentityInequality, Codo.Null),
                    Codo.getEvent(Event.Name).invoke(Codo.This).statement()));
            }
            else
            {
                callbackMethod = generator.genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), name + "ClickCallback");
                callbackMethod.Statements.Add(new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(Codo.getEvent(Event.Name), CodeBinaryOperatorType.IdentityInequality, Codo.Null),
                        Codo.getEvent(Event.Name).invoke(Codo.This).statement()));
            }
            //注册
            generator.initMethod.Statements.Add(Codo.getField(field.Name).getProp(AutoCompoGenerator.NAME_OF_ONCLICK)
                .getMethod(AutoCompoGenerator.NAME_OF_ADDLISTENER).invoke(Codo.getMethod(callbackMethod.Name)).statement());
            //注销
            generator.clearMethod.Statements.Add(Codo.getField(field.Name).getProp(AutoCompoGenerator.NAME_OF_ONCLICK)
                .getMethod(AutoCompoGenerator.NAME_OF_REMOVELISTENER).invoke(Codo.getMethod(callbackMethod.Name)).statement());
        }
    }
    class AnimatorGenerator : ComponentGenerator<Animator>
    {
        public override void onGen(AutoCompoGenerator generator, Animator component, bool isRootComponent, AutoBindFieldInfo info, CodeMemberField field)
        {
            AnimatorController controller = component.runtimeAnimatorController as AnimatorController;
            foreach (var parameter in controller.parameters)
            {
                string fieldName = "ANIM_PARAM_";
                string animatorName = generator.genPropName4Field(field.Name);
                if (animatorName.EndsWith("Animator", StringComparison.OrdinalIgnoreCase))
                    animatorName = animatorName.Substring(0, animatorName.Length - 8);
                if (animatorName.StartsWith("_"))
                    animatorName = animatorName.Substring(1, animatorName.Length - 1);
                if (!(animatorName.StartsWith("as") && char.IsUpper(animatorName[2])))
                    fieldName += animatorName.ToUpper() + "_";
                fieldName += parameter.name.ToUpper();
                CodeMemberField Const = generator.genField("const string", fieldName, false);
                Const.InitExpression = Codo.String(parameter.name);
            }
        }
    }
}