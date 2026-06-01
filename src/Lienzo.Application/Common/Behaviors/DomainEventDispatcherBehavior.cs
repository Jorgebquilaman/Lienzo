using Lienzo.Domain.Common;
using MediatR;

namespace Lienzo.Application.Common.Behaviors;

public class DomainEventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMediator _mediator;

    public DomainEventDispatcherBehavior(IMediator mediator) => _mediator = mediator;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        return response;
    }
}
