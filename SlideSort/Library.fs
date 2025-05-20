namespace SlideSort

module SlideSorter =
    // 定义玻片类型 
    type Slide = {
        Id: int
        ProgramId: string
        PathologyId: string 
        SlideId: string
        DoctorId: int
        DoctorName: string 
        PatientName: string 
        // 可以添加更多属性 
    } 

    // 定义盒子位置信息 
    type Position = { 
        BoxIndex: int   // 盒子索引 (0-74) 
        SlotIndex: int  // 盒子内的位置索引 (0-19) 
    } 

    // 将线性索引转换为盒子位置 
    let toPosition (linearIndex: int) : Position = 
        { 
            BoxIndex = linearIndex / 20 
            SlotIndex = linearIndex % 20 
        } 

    // 将盒子位置转换为线性索引 
    let toLinearIndex (position: Position) : int = 
        position.BoxIndex * 20 + position.SlotIndex 

    // 定义玻片在位置上的映射 
    type SlidePositionMap = { 
        SlideAtPosition: Map<int, Slide>     // 位置 -> 玻片 
        PositionOfSlide: Map<Slide, int>     // 玻片 -> 位置 
    } 

    // 创建空的映射 
    let emptyMap = { 
        SlideAtPosition = Map.empty 
        PositionOfSlide = Map.empty 
    } 

    // 添加玻片到位置的映射 
    let addSlidePosition (slide: Slide) (position: int) (map: SlidePositionMap) : SlidePositionMap = 
        { 
            SlideAtPosition = Map.add position slide map.SlideAtPosition 
            PositionOfSlide = Map.add slide position map.PositionOfSlide 
        } 

    // 移动玻片并更新映射
    let moveSlide (fromPos: int) (toPos: int) (map: SlidePositionMap) : SlidePositionMap =
        match Map.tryFind fromPos map.SlideAtPosition with
        | None -> map // 源位置没有玻片，不做任何操作
        | Some slide ->
            // 从源位置移除玻片，并添加到目标位置
            let updatedSlideAtPosition = 
                map.SlideAtPosition
                |> Map.remove fromPos
                |> Map.add toPos slide
            
            // 更新玻片的位置信息
            let updatedPositionOfSlide =
                map.PositionOfSlide
                |> Map.remove slide
                |> Map.add slide toPos
            
            { SlideAtPosition = updatedSlideAtPosition; PositionOfSlide = updatedPositionOfSlide }

    // 移动操作的结果类型 
    type MoveResult = { 
        FinalMap: SlidePositionMap  // 最终的映射状态 
        Moves: (int * int) list     // 移动操作列表 (源位置, 目标位置) 
    } 

    // 错误类型 
    type SortError = 
        | InsufficientPositions of string 
        | NoEmptyPositionsAvailable of string 
        | OtherError of string 

    // 更新过的主排序算法，考虑位置可用性，使用Result类型处理错误 
    let sortSlides (slides: (Slide * int) list) (sortKey: Slide -> 'a) (availablePositions: int[]) : Result<(int * int) list, SortError> = 
        // 初始化映射 
        let initialMap = 
            List.fold (fun map (slide, pos) -> addSlidePosition slide pos map) emptyMap slides 
    
        // 获取需要排序的玻片和它们当前的位置 
        let slidesToSort = 
            slides 
            |> List.map fst 
            |> List.sortBy sortKey  // 根据指定的键进行排序 
    
        // 获取可用的位置（状态为1或2的位置） 
        let usablePositions = 
            availablePositions 
            |> Array.mapi (fun i state -> (i, state)) 
            |> Array.filter (fun (_, state) -> state = 1 || state = 2) 
            |> Array.map fst 
            |> Set.ofArray 
    
        // 获取空位置（状态为2的位置） 
        let emptyPositions = 
            availablePositions 
            |> Array.mapi (fun i state -> (i, state)) 
            |> Array.filter (fun (_, state) -> state = 2) 
            |> Array.map fst 
            |> Set.ofArray

        let emptyPositionsList = emptyPositions |> Set.toList |> List.sort
        let occupiedUsablePositionsList = 
            Set.difference usablePositions emptyPositions
            |> Set.toList 
            |> List.sort

        // 检查是否有足够的可用位置 
        if usablePositions.Count < slidesToSort.Length then 
            Error (InsufficientPositions (sprintf "可用位置数量(%d)不足以容纳所有玻片(%d)" usablePositions.Count slidesToSort.Length)) 
        else 
            // 优先使用空位置，不足时再使用已占用的可用位置
            let availableForTarget =
                if emptyPositionsList.Length >= slidesToSort.Length then
                    emptyPositionsList |> List.take slidesToSort.Length
                else
                    let fromEmpty = emptyPositionsList
                    let fromOccupied = occupiedUsablePositionsList |> List.take (slidesToSort.Length - emptyPositionsList.Length)
                    fromEmpty @ fromOccupied

            let targetPositions = 
                slidesToSort 
                |> List.mapi (fun i slide -> (slide, availableForTarget.[i])) 
                |> Map.ofList 
        
            // 将当前可用性状态转换为Map以便更新
            let initialAvailability =
                availablePositions
                |> Array.mapi (fun i state -> (i, state))
                |> Map.ofArray
        
            // 递归函数执行移动，使用Result处理错误 
            let rec performMoves (currentMap: SlidePositionMap) (currentAvailability: Map<int, int>) (pendingMoves: (int * int) list) (processed: Set<int>) : Result<MoveResult, SortError> = 
                // 检查是否还有未处理的玻片 
                let unprocessedSlides = 
                    slidesToSort 
                    |> List.filter (fun s -> 
                        let currentPos = Map.find s currentMap.PositionOfSlide 
                        let targetPos = Map.find s targetPositions 
                        currentPos <> targetPos && not (Set.contains currentPos processed)) 
            
                if List.isEmpty unprocessedSlides then 
                    // 所有玻片都在正确位置上 
                    Ok { FinalMap = currentMap; Moves = List.rev pendingMoves } 
                else 
                    // 处理下一个玻片 
                    let slideToMove = List.head unprocessedSlides 
                    let currentPos = Map.find slideToMove currentMap.PositionOfSlide 
                    let targetPos = Map.find slideToMove targetPositions 
                
                    // 检查目标位置是否已经有其他玻片 
                    match Map.tryFind targetPos currentMap.SlideAtPosition with 
                    | Some occupyingSlide when occupyingSlide <> slideToMove -> 
                        // 目标位置被占用，需要找一个临时位置 
                        // 寻找一个空位置 (状态为2的位置) 
                        let currentEmptyPositions = 
                            currentAvailability 
                            |> Map.filter (fun k v -> v = 2) 
                            |> Map.keys 
                            |> Set.ofSeq 
                    
                        // 选择第一个可用空位置作为临时位置 
                        if Set.isEmpty currentEmptyPositions then 
                            Error (NoEmptyPositionsAvailable "没有可用的空位置来临时存放玻片") 
                        else 
                            let tempPos = Set.minElement currentEmptyPositions 
                        
                            // 先将占用目标位置的玻片移到临时位置
                            let occupyingSlideCurrentPos = Map.find occupyingSlide currentMap.PositionOfSlide
                        
                            // 更新映射和可用性
                            let updatedMap = moveSlide occupyingSlideCurrentPos tempPos currentMap
                            let updatedAvailability = 
                                currentAvailability
                                |> Map.add occupyingSlideCurrentPos 2  // 原位置变为空
                                |> Map.add tempPos 1                   // 临时位置变为已占用
                        
                            // 记录移动
                            let updatedMoves = (occupyingSlideCurrentPos, tempPos) :: pendingMoves
                        
                            // 继续处理，但不将当前位置标记为已处理
                            performMoves updatedMap updatedAvailability updatedMoves processed
                
                    | _ -> 
                        // 目标位置为空或已经有正确的玻片，直接移动
                        // 更新映射和可用性
                        let updatedMap = moveSlide currentPos targetPos currentMap
                        let updatedAvailability = 
                            currentAvailability
                            |> Map.add currentPos 2  // 原位置变为空
                            |> Map.add targetPos 1   // 目标位置变为已占用
                    
                        // 记录移动
                        let updatedMoves = (currentPos, targetPos) :: pendingMoves
                    
                        // 将当前位置标记为已处理
                        let updatedProcessed = Set.add currentPos processed
                    
                        // 继续处理下一个玻片
                        performMoves updatedMap updatedAvailability updatedMoves updatedProcessed
        
            // 开始执行移动操作
            match performMoves initialMap initialAvailability [] Set.empty with
            | Ok result -> Ok result.Moves
            | Error e -> Error e

    // 为C#互操作提供的公共接口
    // 这些函数将被C#代码调用
    
    // 定义排序键类型
    type SortKeyType =
        | ByProgramId
        | ByPathologyId
        | BySlideId
        | ByDoctorId
        | ByDoctorName
        | ByPatientName
    
    // 将F#的Result转换为C#友好的结构
    type SortResult =
        | Success of (int * int) array
        | Failure of string

    type SortResultDto = {
        IsSuccess: bool
        Moves: (int * int) array option
        ErrorMessage: string option
    }

    let convertSortResultToDto (result: SortResult) : SortResultDto =
        match result with
        | Success moves -> 
            { IsSuccess = true; Moves = Some moves; ErrorMessage = None }
        | Failure msg -> 
            { IsSuccess = false; Moves = None; ErrorMessage = Some msg }
    
    // 公开的排序函数，接受排序键类型
    let sortSlidesByKey (slides: (Slide * int) array) (keyType: SortKeyType) (availablePositions: int[]) : SortResult =
        let slidesList = Array.toList slides
        
        // 根据键类型选择排序函数
        let sortKeyFunc =
            match keyType with
            | ByProgramId -> fun (slide: Slide) -> slide.ProgramId
            | ByPathologyId -> fun (slide: Slide) -> slide.PathologyId
            | BySlideId -> fun (slide: Slide) -> slide.SlideId
            | ByDoctorId -> fun (slide: Slide) -> slide.DoctorId.ToString()
            | ByDoctorName -> fun (slide: Slide) -> slide.DoctorName
            | ByPatientName -> fun (slide: Slide) -> slide.PatientName
        
        // 调用主排序函数
        match sortSlides slidesList sortKeyFunc availablePositions with
        | Ok moves -> Success (List.toArray moves)
        | Error (InsufficientPositions msg) -> Failure msg
        | Error (NoEmptyPositionsAvailable msg) -> Failure msg
        | Error (OtherError msg) -> Failure msg

    let sortSlidesByKeyForCSharp slides keyType availablePositions =
        sortSlidesByKey slides keyType availablePositions
        |> convertSortResultToDto

    //let genAvailablePositions() = 
    //    // 定义数组总长度 
    //    let totalLength = 1500 
    //    // 定义各个部分的长度和起始/结束索引 
    //    let lenSection1 = 20 // 索引 0 到 19 
    //    let lenSection2 = 40 - 20 + 1 // 索引 20 到 40 (包含20和40，共21个位置) 
    //    let lenSection3 = totalLength - (lenSection1 + lenSection2) // 索引 41 到 1499 (1500 - 20 - 21 = 1459 个位置) 
    //    // --- 创建各个部分的序列 --- 
    //    // Section 1: 前20个位置，值都是2 
    //    let section1 = Seq.init lenSection1 (fun _-> 0) 
    //    // Section 2: 位置20到40，值都是1 
    //    //let section2 = Seq.init lenSection1 (fun _-> 1) 
    //    let section2_arrayLiteral = [|1; 1; 1; 1; 1; 2; 2; 2; 2; 2; 1; 1; 1; 1; 1; 1; 1; 1; 1; 1|] 
    //    // 将数组 literal 转换为序列以便与 Seq.concat 一起使用 
    //    let section2 = Seq.ofArray section2_arrayLiteral 
    //    // Section 3: 位置41到1500 (即索引41到1499)，值都是0 
    //    let section3 = Seq.init lenSection3 (fun _-> 0) 
    //    // --- 合并序列并转换为数组 --- 
    //    let availablePositions : int[] = 
    //        section3 
    //        |> Seq.append section2 // 将section2追加到section1后面 
    //        |> Seq.append section1 // 将section3追加到前面合并的序列后面 
    //        |> Seq.toArray       // 将最终序列转换为数组 
    
    //    availablePositions
