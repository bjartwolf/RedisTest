open System
open StackExchange.Redis

let inline (~~) (x:^a) : ^b = ((^a or ^b) : (static member op_Implicit: ^a -> ^b) x)
type redisCmd = { key: RedisKey; value: string} 

[<EntryPoint>]
let main argv = 
    let p = Diagnostics.Process.Start(@"redis-server.exe","--port 10000")
    let redis = ConnectionMultiplexer.Connect("localhost:10000")
    let db = redis.GetDatabase()
    let script = LuaScript.Prepare("redis.call('set', @key, @value)")
    let loadedScript = script.Load(redis.GetServer("localhost:10000"))
    let readval = async { 
                          let! a = loadedScript.EvaluateAsync(redis.GetDatabase(), 
                                        {key=(~~"bjartwolf2"); value="this is big redis and all"}) 
                                        |> Async.AwaitTask
                          let! x = db.StringGetAsync(~~"bjartwolf2") 
                                        |> Async.AwaitTask
                          printfn "%A" (x.ToString())
                        }
    readval |> Async.Start 
    Console.ReadKey() |> ignore
    p.Kill()
    0 