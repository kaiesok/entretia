using System.Security.Cryptography;
using System.Text;
using Entretia.Api.Ports;
using Konscious.Security.Cryptography;

namespace Entretia.Api.Adapters;

/// <summary>
/// Hachage argon2id — recommandation OWASP pour les mots de passe.
/// Parametres : 64 Mo de memoire, 3 iterations, parallelisme 1.
/// L'empreinte stockee est auto-descriptive :
///   argon2id$m=65536,t=3,p=1$&lt;sel base64&gt;$&lt;hash base64&gt;
/// Les parametres sont relus depuis l'empreinte a la verification :
/// on pourra les renforcer plus tard sans casser les comptes existants.
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int MemoryKb = 65536; // 64 Mo
    private const int Iterations = 3;
    private const int Parallelism = 1;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Compute(password, salt, MemoryKb, Iterations, Parallelism);

        return $"argon2id$m={MemoryKb},t={Iterations},p={Parallelism}" +
               $"${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != "argon2id")
        {
            return false;
        }

        int memoryKb = MemoryKb, iterations = Iterations, parallelism = Parallelism;
        foreach (var param in parts[1].Split(','))
        {
            var kv = param.Split('=');
            if (kv.Length != 2 || !int.TryParse(kv[1], out var value))
            {
                return false;
            }

            switch (kv[0])
            {
                case "m": memoryKb = value; break;
                case "t": iterations = value; break;
                case "p": parallelism = value; break;
                default: return false;
            }
        }

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Compute(password, salt, memoryKb, iterations, parallelism);

        // Comparaison en temps constant : empeche de deviner l'empreinte
        // octet par octet en mesurant le temps de reponse.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Compute(string password, byte[] salt, int memoryKb, int iterations, int parallelism)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKb,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };
        return argon2.GetBytes(HashSizeBytes);
    }
}
