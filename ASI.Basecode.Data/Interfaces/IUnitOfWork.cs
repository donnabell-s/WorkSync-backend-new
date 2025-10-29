using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Unit of Work Interface
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets the database context
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        DbContext Database { get; }
        /// <summary>
        /// Saves the changes to database
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Saves the changes to database asynchronously
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
