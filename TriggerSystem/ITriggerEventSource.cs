namespace TBSGameCore.TriggerSystem
{
    public interface ITriggerEventSource : ITriggerScope
    {
        string[] getEventNames();
        void addTrigger(string eventName, TriggerComponent trigger);
        void removeTrigger(string eventName, TriggerComponent trigger);
    }
}
