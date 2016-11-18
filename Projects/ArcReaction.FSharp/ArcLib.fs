namespace ArcReaction

    [<AutoOpen>]
    module utils =
        
        open Prelude
        open System
        open System.IO
        open System.Web        
        open System.Collections.Generic

        type String with
            member inline x.is_null_or_whitespace with get () = String.IsNullOrWhiteSpace x     
        
        let deref x = !x //we'll probably hardly ever use reference cells, so lets free up this identifier for something else

        let inline (!) (proc : Prelude.Proc< ^t>) = proc.Execute()

        let inline (++) (s : ^b) (t : ^a) = s.ToString() + (t.ToString())

        let inline parse_int s = Int32.Parse s

        let inline init< ^t> () = Prelude.Default< ^t>().Value
        
        let inline app obj = obj :> AppState
        
        let inline rep obj = (^a : (static member op_Implicit : ^a -> Representation) obj)

        let zip (files : (string * string) list)  =
            let rec zip' (files' : (string * string) list) =            
                match files' with
                | [] -> []
                | x::xs ->
                    let name, path = x
                    let fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    ZipEntry(name, fs) :: (zip' xs)

            new ZipFile(zip' files)
        
        let inline file_as (name : string) (disposition : ArcReaction.FileRepresentation.ContentDisposition) (stream : System.IO.Stream) = ArcReaction.FileRepresentation (name, stream, disposition) :> Representation        
        
        let inline attach_as name stream = file_as name FileRepresentation.Attachment stream
        
        let inline json obj = (^a : (static member op_Implicit : ^a -> JSON) obj)
        
        let inline view (s : string) = Representation.op_Implicit s

        let inline view_model path model = 
            let model = ModelView< ^a>.op_Implicit (path, model) :> IHttpHandler

            Representation.Create(model)

        let inline str s = s.ToString()

        let inline found s = Found(s) |> app
        
        type Attempt<'a> = Success of 'a | Failed

        let inline parse_as< ^a when ^a : (static member TryParse : string  * ^a byref -> bool)> s = 
            
            let mutable value = Unchecked.defaultof< ^a>
             
            let result = (^a : (static member TryParse : string * ^a byref ->  bool ) (s, &value))
            
            if result then Success value else Failed

        let (|Int|NonInt|) s = match Int32.TryParse s with true, v -> Int v | _ -> NonInt s

        let see_other s = SeeOther(s) |> rep

        let inline nullable< ^t when ^t:(new : unit -> ^t) and ^t:>ValueType and ^t:struct> x = new Nullable< ^t>(x)

        let inline (?) x = nullable x

        let inline join_str sep (xs : IEnumerable< ^a>) = String.Join(sep, xs)

        let inline get_context ctx = if ctx = null then HttpContext.Current else ctx

        let get_tld ctx = 
            
            let ctx' = get_context ctx

            ctx'.Request.Url.Host.Split '.' |> Seq.last

        let get_protocol ctx =

            let ctx' = get_context ctx

            ctx'.Request.Url.Scheme

       
        type ReadFormAttempt<'T> =
        | Success of 'T
        | Failed of string

        type CheckCapability<'T> =
        | Confirmed of 'T
        | NotConfirmed of string


        type CapabilityBuilder() =
            member x.Bind (check : CheckCapability<'T>, f : 'T -> CheckCapability<'U>) =
                match check with
                | Confirmed t -> f t
                | NotConfirmed s -> NotConfirmed s

            member x.Return (obj) = Confirmed obj

            member x.Zero() = NotConfirmed "Unknown."

        let capabilities = new CapabilityBuilder()       

//        type ISO.Web.Security.ISOUser with
//            member x.Check<'t when 't : equality and 't :> Capability and 't : null>() = 
//                let cap = x.GetCapability<'t>()
//                if cap = null then NotConfirmed typedefof<'t>.Name else Confirmed cap
        

        type FormletBuilder(ctx : HttpContextEx) =
            
            member x.get_int (s : string) = 
                match Int32.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success (Nullable<int>(n))
                | _ -> Success (Nullable<int>())
                
            member x.get_uint (s : string) = 
                match Int64.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success (Nullable<Int64>(n))
                | _ -> Success (Nullable<Int64>())

            member x.get_datetime (s : string) = 
                match DateTime.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success (Nullable<DateTime>(n))
                | _ -> Success (Nullable<DateTime>())

            member x.get_bool (s : string) = 
                match Boolean.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success (Nullable<Boolean>(n))
                | _ -> Success (Nullable<Boolean>())

            member x.get_guid (s : string) = 
                match Guid.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success (Nullable<Guid>(n))
                | _ -> Success (Nullable<Guid>())
            
            member x.get_string (s : string) = ctx.[s]                

            member x.try_int (s : string) = 
                match Int32.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success n
                | _ -> Failed s

            member x.try_uint (s : string) = 
                match Int64.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success n
                | _ -> Failed s

            member x.try_datetime (s : string) = 
                match DateTime.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success n
                | _ -> Failed s

            member x.try_guid (s : string) = 
                match Guid.TryParse (ctx.Request.[s]) with
                | (true, n) -> Success n
                | _ -> Failed s

            member x.try_bool (s : string) = 

                if s.Equals("yes", StringComparison.OrdinalIgnoreCase) 
                then 
                    Success true
                else
                    if s.Equals("no", StringComparison.OrdinalIgnoreCase)
                    then
                        Success false
                        else                
                            match Boolean.TryParse (ctx.Request.[s]) with
                            | (true, n) -> Success n
                            | _ -> Failed s

            member x.try_nonempty_string (s: string) =
                let s' = ctx.[s]
                if String.IsNullOrEmpty(s') then Failed s else Success s'
            
          
            member x.Bind(read : ReadFormAttempt<'T>, f : 'T -> ReadFormAttempt<'U>) =
                match read with
                | Success p -> f p
                | Failed s -> Failed s

            member x.Return (obj) = Success obj
            member x.Zero () = Failed "unknown"

        
        let read ctx = new FormletBuilder(ctx)
        

        let get_str (s) =
            let r = HttpContext.Current.Request.[s]
            if r.is_null_or_whitespace then Failed s else Success r

        let maybe_int s =
            let (result, n) = Int32.TryParse(HttpContext.Current.Request.[s])

            Success (if result then new Nullable<int>(n) else new Nullable<int>())

        let get_decimal s =
            match Decimal.TryParse(HttpContext.Current.Request.[s]) with
            | (true, d) ->  Success d
            | _ -> Failed s
        
        
        let get_guid (s) =
            match Guid.TryParse(HttpContext.Current.Request.[s]) with
            | (true, d) -> Success d
            | _ -> Failed s
        
        
        let get_date (s) =
            match DateTime.TryParse(HttpContext.Current.Request.[s]) with
            | (true, d) -> Success d
            | _ -> Failed s
        
        
        let maybe_str (s) =
            match get_str s with
            | Success s' -> Success s'
            | Failed s' -> Success null
            

        let get_int (s) = 
            match Int32.TryParse (HttpContext.Current.Request.[s]) with
            | (true, n) -> Success n 
            | _ -> Failed s
            
         
        let get_positive_int s =
            match get_int s with
            | Success n -> if n > 0 then Success n else Failed "Expected an integer greater than zero"
            | Failed s -> Failed s
