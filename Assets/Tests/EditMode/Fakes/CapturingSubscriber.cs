// 최소한의 테스트용 구독자: Raise를 호출하면 구독한 모두에게 메시지를 전달한다.

using System;
using System.Collections.Generic;
using MessagePipe;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class CapturingSubscriber<T> : ISubscriber<T>
    {
        private readonly List<IMessageHandler<T>> _handlers = new();

        public IDisposable Subscribe(IMessageHandler<T> handler, params MessageHandlerFilter<T>[] filters)
        {
            _handlers.Add(handler);
            return new Unsubscriber(() => _handlers.Remove(handler));
        }

        public void Raise(T message)
        {
            for (int handlerIndex = 0; handlerIndex < _handlers.Count; handlerIndex++)
            {
                _handlers[handlerIndex].Handle(message);
            }
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly Action _dispose;

            public Unsubscriber(Action dispose) => _dispose = dispose;

            public void Dispose() => _dispose();
        }
    }
}
