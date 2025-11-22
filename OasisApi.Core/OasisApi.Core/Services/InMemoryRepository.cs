using OasisApi.Core.Models;
using System.Xml.Linq;

public class InMemoryRepository
{
    private readonly List<Sentence> _sentences = new List<Sentence>();

    public InMemoryRepository()
    {
        // Dados de inicialização
        _sentences.Add(new Sentence { Text = "Esta é uma ótima ideia!", IsPositive = true });
        _sentences.Add(new Sentence { Text = "Não gostei do resultado.", IsPositive = false });
    }

    public Sentence Add(Sentence item)
    {
        _sentences.Add(item);
        return item;
    }

    public List<Sentence> GetAll() => _sentences.ToList();

    public Sentence? GetById(Guid id) => _sentences.FirstOrDefault(s => s.Id == id);

    public Sentence? Update(Guid id, Sentence updatedItem)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        existing.Text = updatedItem.Text;
        existing.IsPositive = updatedItem.IsPositive;
        return existing;
    }

    public bool Delete(Guid id)
    {
        var existing = GetById(id);
        if (existing == null) return false;
        return _sentences.Remove(existing);
    }
}