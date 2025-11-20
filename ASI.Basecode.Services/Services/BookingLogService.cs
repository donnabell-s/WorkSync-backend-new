using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ASI.Basecode.Services.Services
{
    public class BookingLogService : IBookingLogService
    {
        private readonly IBookingLogRepository _bookingLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingLogService(IBookingLogRepository bookingLogRepository, IUnitOfWork unitOfWork)
        {
            _bookingLogRepository = bookingLogRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<BookingLog> GetBookingLogs() => _bookingLogRepository.GetBookingLogs();

        public IQueryable<BookingLog> GetByBookingId(int bookingId) => _bookingLogRepository.GetBookingLogs().Where(b => b.BookingIdString == bookingId.ToString());

        public BookingLog GetById(int bookingLogId) => _bookingLogRepository.GetById(bookingLogId);

        public void Create(BookingLog log)
        {
            // Let the database generate BookingLogId (identity)
            _bookingLogRepository.Add(log);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int bookingLogId)
        {
            var entity = _bookingLogRepository.GetById(bookingLogId);
            if (entity == null) return;
            _bookingLogRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }

        public async Task<List<BookingLog>> GetBookingLogsAsync(CancellationToken cancellationToken = default)
        {
            return await _bookingLogRepository.GetBookingLogsAsync(cancellationToken);
        }

        public async Task<List<BookingLog>> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await _bookingLogRepository.GetByBookingIdAsync(bookingId, cancellationToken);
        }

        public async Task<BookingLog> GetByIdAsync(int bookingLogId, CancellationToken cancellationToken = default)
        {
            return await _bookingLogRepository.GetByIdAsync(bookingLogId, cancellationToken);
        }

        public async Task CreateAsync(BookingLog log, CancellationToken cancellationToken = default)
        {
            // Let the database generate BookingLogId (identity)
            await _bookingLogRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int bookingLogId, CancellationToken cancellationToken = default)
        {
            var entity = await _bookingLogRepository.GetByIdAsync(bookingLogId, cancellationToken);
            if (entity == null) return;
            _bookingLogRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private void AddLog(int bookingId, string changeType, string message, int? actorId)
        {
            try
            {
                Console.WriteLine($"[AddLog] Invoked with bookingId={bookingId}, changeType={changeType}, message={message}, actorId={actorId}");

                // Use the connection string directly
                var connectionString = "Addr=localhost; database=WorkSync_db; Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
                {
                    conn.Open();

                    int nextId;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ISNULL(MAX(BookingLogId),0)+1 FROM ws.BookingLogs";
                        nextId = Convert.ToInt32(cmd.ExecuteScalar() ?? 1);
                    }
                    Console.WriteLine($"[AddLog] Next BookingLogId={nextId}");

                    string bookingName = null;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TOP 1 Title FROM ws.Bookings WHERE BookingId = @p0";
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", bookingId));
                        var result = cmd.ExecuteScalar();
                        bookingName = result?.ToString();
                    }
                    Console.WriteLine($"[AddLog] Resolved bookingName={bookingName}");

                    string authorName = null;
                    if (actorId.HasValue)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT TOP 1 Fname + ' ' + Lname FROM ws.Users WHERE Id = @p0";
                            cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", actorId.Value));
                            var result = cmd.ExecuteScalar();
                            authorName = result?.ToString();
                        }
                    }
                    Console.WriteLine($"[AddLog] Resolved authorName={authorName}");

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO [ws].[BookingLogs](BookingLogId, BookingIdString, BookingName, AuthorId, AuthorName, EventType, CurrentStatus, Timestamp) VALUES (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7)";
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", nextId));
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p1", bookingId.ToString()));
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p2", (object)bookingName ?? DBNull.Value));
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p3", (object)actorId ?? DBNull.Value));
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p4", (object)authorName ?? DBNull.Value));
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p5", (object)changeType ?? DBNull.Value));
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p6", (object)message ?? DBNull.Value));
                        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p7", DateTime.UtcNow));
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("[AddLog] Log successfully inserted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddLog] Failed to insert log: {ex.Message}");
            }
        }
    }
}
