using Lienzo.Application.DTOs;

namespace Lienzo.Application.Interfaces;

public interface IReservationPdfGenerator
{
    byte[] Generate(ReservationPdfModel model);
}