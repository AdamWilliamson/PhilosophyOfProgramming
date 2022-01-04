//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Utilities
//{
//    public delegate void ObjectChanged<TIn>(TIn obj);

//    class Model { }

//    interface IListener
//    {
//        void Execute(object input);
//    }

//    class Listener<TIn> : IListener
//    {
//        event ObjectChanged<TIn> ChangedEvent;

//        public Listener(ObjectChanged<TIn> listener)
//        {
//            ChangedEvent = listener;
//        }

//        public void Execute(object input)
//        {
//            //ChangedEvent(input);
//        }
//    }

//    class EventDispatcher
//    {
//        Dictionary<Type, IListener> Listeners = new Dictionary<Type, IListener>();

//        public void AddListener<TIn>(ObjectChanged<TIn> callback)
//        {
//            Listeners.Add(typeof(TIn), new Listener<TIn>(callback));
//        }

//        public void SendEvent<T>(T obj)
//        {
//            //Listeners[typeof(T)].Execute<T>(obj);
//        }
//    }

//    class Test(){

//        public void ModelChanged(Model m)
//    {

//    }

//    public void Run()
//    {

//        EventDispatcher dispacher = new EventDispatcher();
//        dispacher.AddListener<Model>(f => ModelChanged(f));
//    }

//    class DomainNotificationService
//    {
//        class Convert
//        {
//            Dictionary<Type, IObjectConverter> Converters = new Dictionary<Type, IObjectConverter>();

//            public TOut Get<TOut>(TIn obj)
//            {
//                return (TOut)Converters[typeof(TIn)].Convert(obj);
//            }
//        }

//        Dictionary<Type, Convert>

//        public void AddConverter<TIn, TOut>(IConverter<TIn, TOut> converter)
//        {
//            Converters.Add(typeof(TOut), converter);
//        }

//        public void Convert<TOut>(TIn obj)
//        {

//        }
//    }
//}
