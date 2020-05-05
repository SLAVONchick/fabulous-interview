// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Collections.Generic

[<Literal>]
let Menu = """
Commands:
1. Add user;
2. Remove user;
3. Send message from user;
4. Get messages for user;
5. Exit.
Enter command number to execute:"""  


[<StructuredFormatDisplay("New message from \"{From}\": \"{Message}\"")>]
type Message =
    { From: string
      Message: string }

let server = Dictionary<string, ResizeArray<Message>>() 

let execute cmd =
    match cmd with
    | 1 ->
        printfn "Enter user name to add:"
        let name = Console.ReadLine()
        if name = nameof server
        then printfn "Name should not be \"%s\"" <| nameof server
        elif not <| server.TryAdd(name, ResizeArray<Message>())
        then printfn "User already exists!"
        else
            for userName in server.Keys do
                if userName <> name
                then server.[userName].Add { From = nameof server
                                             Message = sprintf "User \"%s\" joined the server" name }
        true
    | 2 ->
        printfn "Enter user name to remove:"
        let name = Console.ReadLine()
        if not <| server.Remove name
        then printfn "User does not exist!"
        else
            for userName in server.Keys do
                server.[userName].Add { From = nameof server
                                        Message = sprintf "User \"%s\" left the server" name }
        true
    | 3 ->
        printfn "Enter name of the user that wants to send a message:"
        let senderName = Console.ReadLine()
        if not <| server.ContainsKey senderName
        then printfn "User does not exist!"
        else
            printfn "Enter name of the user that will receive the message:"
            let receiverName = Console.ReadLine()
            let (success, receiverMessages) = server.TryGetValue receiverName 
            if not success
            then printfn "User does not exist!"
            else
                printfn "Enter the message you want to send:"
                let msg = Console.ReadLine()
                receiverMessages.Add { From = senderName; Message = msg }
                printfn "Message sent!"
        true
    | 4 ->
        printfn "Enter the user name:"
        let name = Console.ReadLine()
        let (success, messages) = server.TryGetValue name
        if not success
        then printfn "User does not exist!"
        else
            Seq.iter (printfn "%A") messages
            messages.Clear()
        true
    | 5 -> false
    | _ ->
        printfn "Unknown command!"
        true
let rec mainLoop cont =
    if cont
    then
        printfn "%s" Menu
        let cmd = Console.ReadLine()
        let (success, cmd) = Int32.TryParse cmd
        if not success
        then printfn "The command must be a number!"
        else mainLoop (execute cmd) 
  

[<EntryPoint>]
let main argv =
    mainLoop true
    0 // return an integer exit code