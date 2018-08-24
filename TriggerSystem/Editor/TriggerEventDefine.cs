namespace TBSGameCore.TriggerSystem
{
    class TriggerEventDefine
    {
        public TriggerEventDefine(ITriggerEventSource source, string eventName)
        {
            eventSource = source;
            this.eventName = eventName;
        }
        public void addTrigger(TriggerComponent trigger)
        {
            //if (eventSource != null)
            //    trigger.setEvent(eventSource, eventName);
        }
        public void removeTrigger(TriggerComponent trigger)
        {
            if (eventSource != null)
                eventSource.removeTrigger(eventName, trigger);
        }
        ITriggerEventSource eventSource { get; set; }
        public string eventName { get; private set; }
    }
}
