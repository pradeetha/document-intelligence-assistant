using EnterpriseAI.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EnterpriseAI.Api.Data;

namespace EnterpriseAI.Api.Data;

public class EnterpriseAiDbContext(
    DbContextOptions<EnterpriseAiDbContext> options)
    : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<ChatMessage>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Conversation>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(x => new
            {
                x.ConversationId,
                x.CreatedAt
            });
        modelBuilder.Entity<Document>()
            .HasMany(x => x.Chunks)
            .WithOne(x => x.Document)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}