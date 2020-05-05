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

type ConsoleMessengerError =
    | UserNameEqualsServer
    | UserAlreadyExists
    | UserDoesNotExist
    | UnknownCommand
    with
    override self.ToString() =
        match self with
        | UserNameEqualsServer -> sprintf "Name should not be \"%s\"" (nameof server)
        | UserAlreadyExists -> "User already exists!"
        | UserDoesNotExist -> "User does not exist!"
        | UnknownCommand -> "Unknown command!"

module ConsoleMessengerError =    
    let print (err: ConsoleMessengerError) = err.ToString() 
        

let execute cmd =
    match cmd with
    | 1 ->
        printfn "Enter user name to add:"
        let name = Console.ReadLine()
        if name = nameof server
        then Error UserNameEqualsServer
        elif not <| server.TryAdd(name, ResizeArray<Message>())
        then Error UserAlreadyExists
        else
            for userName in server.Keys do
                if userName <> name
                then server.[userName].Add { From = nameof server
                                             Message = sprintf "User \"%s\" joined the server" name }
            Ok "User successfully added"
        |> Some
    | 2 ->
        printfn "Enter user name to remove:"
        let name = Console.ReadLine()
        if not <| server.Remove name
        then Error UserDoesNotExist
        else
            for userName in server.Keys do
                server.[userName].Add { From = nameof server
                                        Message = sprintf "User \"%s\" left the server" name }
            Ok "User successfully removed!"
        |> Some
    | 3 ->
        printfn "Enter name of the user that wants to send a message:"
        let senderName = Console.ReadLine()
        if not <| server.ContainsKey senderName
        then Error UserDoesNotExist
        else
            printfn "Enter name of the user that will receive the message:"
            let receiverName = Console.ReadLine()
            let (success, receiverMessages) = server.TryGetValue receiverName 
            if not success
            then Error UserDoesNotExist
            else
                printfn "Enter the message you want to send:"
                let msg = Console.ReadLine()
                receiverMessages.Add { From = senderName; Message = msg }
                Ok "Message sent!"
        |> Some
    | 4 ->
        printfn "Enter the user name:"
        let name = Console.ReadLine()
        let (success, messages) = server.TryGetValue name
        if not success
        then Error UserDoesNotExist
        else
            Seq.iter (printfn "%A") messages
            messages.Clear()
            Ok "All messages printed."
        |> Some
    | 5 -> None
    | _ ->
        Some <| Error UnknownCommand
let rec mainLoop cont =
    match cont with
    | None -> ()
    | Some result ->
        result
        |> Result.map (printfn "%s")
        |> Result.mapError (ConsoleMessengerError.print >> printfn "%s")
        |> ignore
        printfn "%s" Menu
        let cmd = Console.ReadLine()
        let (success, cmd) = Int32.TryParse cmd
        if not success
        then printfn "The command must be a number!"
        else mainLoop (execute cmd) 
  

[<EntryPoint>]
let main argv =
    mainLoop (Some <| Ok "starting server...")
    0 // return an integer exit code