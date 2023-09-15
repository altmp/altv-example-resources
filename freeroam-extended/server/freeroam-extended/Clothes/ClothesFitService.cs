using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Freeroam_Extended.Clothes;

internal class IdGenerator
{
    private Stack<long> idStack;

    public IdGenerator()
    {
        this.idStack = new Stack<long>();
        this.PushId(long.MinValue);
    }

    public long GetNextId()
    {
        if (this.idStack.Count == 0)
        {
            // If the stack is empty, push the next sequential ID.
            this.PushId(this.idStack.Count + 1);
        }

        return this.idStack.Pop();
    }

    public void ReleaseId(long id)
    {
        this.idStack.Push(id);
    }

    private void PushId(long id)
    {
        this.idStack.Push(id);
    }
}

internal static class ClothesFitService
{
    private static readonly IdGenerator _idGen = new();
    private static readonly Dictionary<long, TaskCompletionSource<object>> _tasks = new();

    static ClothesFitService()
    {
        Alt.OnServer("clothes.resp", (long id, string type, object result) =>
        {
            if (!_tasks.TryGetValue(id, out var tcs))
            {
                return;
            }

            _tasks.Remove(id);

            if (type == "error")
            {
                tcs.TrySetException(new Exception(result?.ToString()));
            }
            else
            {
                tcs.SetResult(result);
            }
        });
    }

    public static Task InitPlayer(IPlayer player)
    {
        return MakeReq(player, "initPlayer");
    }

    public static Task DestroyPlayer(IPlayer player)
    {
        return MakeReq(player, "destroyPlayer");
    }

    public static Task Equip(IPlayer player, string hash, bool force = true)
    {
        //TODO CLOTHES IMPLEMENT FORCE ON CLIENT
        return MakeReq(player, "equip", hash, force);
    }

    public static Task Equip(IPlayer player, uint hash, bool force = true)
    {
        //TODO CLOTHES IMPLEMENT FORCE ON CLIENT
        return MakeReq(player, "equip", hash, force);
    }

    public static Task UnEquip(IPlayer player, string hash)
    {
        return MakeReq(player, "unequip", hash);
    }

    public static Task UnEquip(IPlayer player, uint hash)
    {
        return MakeReq(player, "unequip", hash);
    }

    public static async Task<ulong[]> GetOutfitsBySex(uint sex)
    {
        var tcs = new TaskCompletionSource<object>();
        var id = _idGen.GetNextId();
        _tasks.Add(id, tcs);
        Alt.Emit("clothes.req", null, "getoutfits", id, sex);
        var result = await tcs.Task;
        return ((object[]) result).Select(Convert.ToUInt64).ToArray();
    }

    private static Task MakeReq(IPlayer player, string eventName, params object[] args)
    {
        var tcs = new TaskCompletionSource<object>();
        var id = _idGen.GetNextId();
        _tasks.Add(id, tcs);
        Alt.Emit("clothes.req", player, eventName, id, args);
        return tcs.Task;
    }
}