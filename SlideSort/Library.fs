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
        | InsufficientUsablePositions of string     // Not enough state 1 or 2 positions overall
        | NoEmptyPositionForSwap of string        // At least one state 2 needed for swaps if not already sorted
        | NoEmptyPositionsAvailableForTempMove of string // In performMoves, cannot find temp empty spot
        | SlideNotInMap of string                   // 玻片不在预期的当前位置映射中
        | TargetPositionUnavailable of string       // 目标位置状态不是预期的 (例如状态为0或不在状态表)
        | SlideNotExpectedInTargetPositions of string // 玻片不在预期的最终目标映射中 (内部逻辑错误)
        | OtherError of string                      // 其他内部逻辑错误

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
            Error (InsufficientUsablePositions (sprintf "可用位置数量(%d)不足以容纳所有玻片(%d)" usablePositions.Count slidesToSort.Length)) 
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
                            Error (NoEmptyPositionsAvailableForTempMove "没有可用的空位置来临时存放玻片") 
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

    // 主排序算法
    let sortSlidesNew (slides: (Slide * int) list) (sortKey: Slide -> 'a) (initialAvailablePositions: int[]) : Result<(int * int) list, SortError> =
        // 初始化玻片位置映射
        let initialMap =
            List.fold (fun map (slide, pos) -> addSlidePosition slide pos map) emptyMap slides

        // 获取需要排序的玻片，并按指定键排序
        let slidesToSort =
            slides
            |> List.map fst
            |> List.sortBy sortKey

        if List.isEmpty slidesToSort then
            Ok [] // 没有需要排序的玻片，无需操作
        else
            // 将初始位置状态数组转换为Map
            let initialAvailabilityMap =
                initialAvailablePositions
                |> Array.mapi (fun i state -> (i, state))
                |> Map.ofArray

            // 确定排序后玻片的最终物理目标位置
            // 这些是按索引排序的前 N 个可用 (初始状态为1或2) 的物理位置
            let allInitiallyUsableSortedPhysicalPositions =
                initialAvailabilityMap
                |> Map.toSeq
                |> Seq.filter (fun (_, state) -> state = 1 || state = 2) // 初始状态为1或2的位置
                |> Seq.map fst
                |> Seq.toList
                |> List.sort

            if allInitiallyUsableSortedPhysicalPositions.Length < slidesToSort.Length then
                Error (InsufficientUsablePositions (sprintf "总共可用(状态1或2)的位置数量 (%d) 不足以容纳所有待排序玻片 (%d)."
                                                     allInitiallyUsableSortedPhysicalPositions.Length slidesToSort.Length))
            else
                let finalTargetPhysicalSlotsForSortedSlides =
                    allInitiallyUsableSortedPhysicalPositions
                    |> List.take slidesToSort.Length

                // 检查玻片是否已经按其最终目标位置排好序
                let currentlyInPlaceAndSorted =
                    slidesToSort
                    |> List.mapi (fun i sortedSlide ->
                        match Map.tryFind sortedSlide initialMap.PositionOfSlide with
                        | Some currentPos -> currentPos = finalTargetPhysicalSlotsForSortedSlides.[i]
                        | None -> false // 防御：如果玻片不在初始映射中，则认为未就位
                    )
                    |> List.forall id

                if currentlyInPlaceAndSorted then
                    Ok [] // 已经按目标排好序，无需移动
                else
                    // 如果未排序，检查是否有至少一个空闲位置用于交换
                    let emptyInitialPositions =
                        initialAvailabilityMap
                        |> Map.toSeq
                        |> Seq.filter (fun (_, state) -> state = 2) // 初始状态为2的空闲位置
                        |> Seq.map fst
                        |> Seq.toList

                    if emptyInitialPositions.Length < 1 then
                        Error (NoEmptyPositionForSwap "至少需要一个空闲位置(状态2)来进行交换排序，但目前没有空闲位置且玻片尚未按目标位置排好.")
                    else
                        // 创建玻片到其最终目标位置的映射
                        let targetPositionsMap =
                            slidesToSort
                            |> List.mapi (fun i slide -> (slide, finalTargetPhysicalSlotsForSortedSlides.[i]))
                            |> Map.ofList

                        // 递归函数执行移动
                        let rec performMovesRecursive
                            (currentMap: SlidePositionMap)
                            (currentAvailability: Map<int, int>)
                            (targetPositionsMap: Map<Slide, int>)      // 玻片 -> 最终目标位置
                            (accumulatedMoves: (int * int) list)
                            (slidesRemainingToPlace: Slide list)
                            : Result<MoveResult, SortError> =

                            match slidesRemainingToPlace with
                            | [] ->
                                Ok { FinalMap = currentMap; Moves = List.rev accumulatedMoves }

                            | slideToPlace :: restOfSlides ->
                                match Map.tryFind slideToPlace targetPositionsMap with
                                | None ->
                                    Error (SlideNotExpectedInTargetPositions (sprintf "内部错误: 玻片 %A 未在目标位置映射中找到." slideToPlace))
                                | Some targetPosOfSlideToPlace ->
                                    match Map.tryFind slideToPlace currentMap.PositionOfSlide with
                                    | None -> Error (SlideNotInMap (sprintf "玻片 %A (ID: %s) 在当前玻片位置映射中未找到." slideToPlace slideToPlace.SlideId))
                                    | Some currentPosOfSlideToPlace ->
                                        if currentPosOfSlideToPlace = targetPosOfSlideToPlace then
                                            performMovesRecursive currentMap currentAvailability targetPositionsMap accumulatedMoves restOfSlides
                                        else
                                            match Map.tryFind targetPosOfSlideToPlace currentAvailability with
                                            | None -> Error (TargetPositionUnavailable (sprintf "目标位置 %d 不在可用性状态映射中." targetPosOfSlideToPlace))
                                            | Some targetPosState ->
                                                if targetPosState = 2 then // 目标位置是空闲的
                                                    let newMap = moveSlide currentPosOfSlideToPlace targetPosOfSlideToPlace currentMap
                                                    let newAvailability =
                                                        currentAvailability
                                                        |> Map.add currentPosOfSlideToPlace 2
                                                        |> Map.add targetPosOfSlideToPlace 1
                                                    let newMoves = (currentPosOfSlideToPlace, targetPosOfSlideToPlace) :: accumulatedMoves
                                                    performMovesRecursive newMap newAvailability targetPositionsMap newMoves restOfSlides
                                                elif targetPosState = 1 then // 目标位置被其他玻片占用 (state 1)
                                                    match Map.tryFind targetPosOfSlideToPlace currentMap.SlideAtPosition with
                                                    | Some blockingSlide -> // 目标位置被我们当前管理的另一个玻片 blockingSlide 占用
                                                        let findTempEmptyPos () = 
                                                            currentAvailability
                                                            |> Map.toSeq
                                                            |> Seq.tryFind (fun (_, state) -> state = 2)
                                                            |> Option.map fst

                                                        match findTempEmptyPos () with
                                                        | None -> Error (NoEmptyPositionsAvailableForTempMove "没有可用的临时空闲位置(状态2)来移动被阻挡的玻片.")
                                                        | Some tempEmptyPos ->
                                                            // 步骤 1: 移动 blockingSlide 到 tempEmptyPos
                                                            let mapAfterBlockingMove = moveSlide targetPosOfSlideToPlace tempEmptyPos currentMap
                                                            let availabilityAfterBlockingMove =
                                                                currentAvailability
                                                                |> Map.add targetPosOfSlideToPlace 2
                                                                |> Map.add tempEmptyPos 1
                                                            let movesAfterBlockingMove = (targetPosOfSlideToPlace, tempEmptyPos) :: accumulatedMoves

                                                            // 步骤 2: 移动 slideToPlace 到 targetPosOfSlideToPlace
                                                            let mapAfterMainMove = moveSlide currentPosOfSlideToPlace targetPosOfSlideToPlace mapAfterBlockingMove
                                                            let availabilityAfterMainMove =
                                                                availabilityAfterBlockingMove
                                                                |> Map.add currentPosOfSlideToPlace 2
                                                                |> Map.add targetPosOfSlideToPlace 1 
                                                            let movesAfterMainMove = (currentPosOfSlideToPlace, targetPosOfSlideToPlace) :: movesAfterBlockingMove
                                                            performMovesRecursive mapAfterMainMove availabilityAfterMainMove targetPositionsMap movesAfterMainMove restOfSlides
                                                    | None -> // 目标位置状态为1，但并非被我们当前管理的玻片占用（可能被“外部”实体占用）
                                                        let findTempEmptyPosExternal () =
                                                            currentAvailability
                                                            |> Map.toSeq
                                                            |> Seq.tryFind (fun (_, state) -> state = 2) // Find any empty spot
                                                            |> Option.map fst

                                                        match findTempEmptyPosExternal () with
                                                        | None -> Error (NoEmptyPositionsAvailableForTempMove (sprintf "目标位置 %d 被外部实体占用，且没有临时空位可用于中转该外部实体." targetPosOfSlideToPlace))
                                                        | Some tempEmptyPosForExternal ->
                                                            // 步骤 1: “概念上”移动外部实体从 targetPosOfSlideToPlace 到 tempEmptyPosForExternal
                                                            // 这个移动不更新 currentMap，因为它涉及外部实体，但更新 currentAvailability 和 moves
                                                            let availabilityAfterExternalMove =
                                                                currentAvailability
                                                                |> Map.add targetPosOfSlideToPlace 2      // 目标位置变为空闲
                                                                |> Map.add tempEmptyPosForExternal 1  // 临时空位被外部实体占用
                                                            // 我们记录这个物理移动，即使它涉及一个“未知”玻片
                                                            let movesAfterExternalMove = (targetPosOfSlideToPlace, tempEmptyPosForExternal) :: accumulatedMoves
                                                            // 步骤 2: 现在 targetPosOfSlideToPlace (逻辑上)是空闲的，移动 slideToPlace 到那里
                                                            let mapAfterMainMove = moveSlide currentPosOfSlideToPlace targetPosOfSlideToPlace currentMap
                                                            let availabilityAfterMainMove =
                                                                availabilityAfterExternalMove // 从上一步的状态开始
                                                                |> Map.add currentPosOfSlideToPlace 2  // slideToPlace 的原位置变为空闲
                                                                |> Map.add targetPosOfSlideToPlace 1   // slideToPlace 的目标位置现在被它占用
                                                            let movesAfterMainMove = (currentPosOfSlideToPlace, targetPosOfSlideToPlace) :: movesAfterExternalMove
                                                            performMovesRecursive mapAfterMainMove availabilityAfterMainMove targetPositionsMap movesAfterMainMove restOfSlides
                                                else // targetPosState = 0 (不可用) or other unexpected state
                                                    Error (TargetPositionUnavailable (sprintf "目标位置 %d 不可用 (状态 %d)." targetPosOfSlideToPlace targetPosState))

                        performMovesRecursive initialMap initialAvailabilityMap targetPositionsMap [] slidesToSort
                        |> Result.map (fun moveResult -> moveResult.Moves)

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
    let sortSlidesByKey (slides: (Slide * int) array) (keyType: SortKeyType) (availablePositions: int[]) (newVersion: bool): SortResult =
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
        
        let sortFunc = if newVersion then sortSlidesNew else sortSlides
        // 调用主排序函数
        match sortFunc slidesList sortKeyFunc availablePositions with
        | Ok moves -> Success (List.toArray moves)
        | Error (InsufficientUsablePositions msg) -> Failure msg
        | Error (NoEmptyPositionForSwap msg) -> Failure msg
        | Error (NoEmptyPositionsAvailableForTempMove msg) -> Failure msg
        | Error (SlideNotInMap msg) -> Failure msg
        | Error (TargetPositionUnavailable msg) -> Failure msg
        | Error (SlideNotExpectedInTargetPositions msg) -> Failure msg
        | Error (OtherError msg) -> Failure msg

    let sortSlidesByKeyForCSharp slides keyType availablePositions =
        sortSlidesByKey slides keyType availablePositions false
        |> convertSortResultToDto

    let sortSlidesByKeyForCSharpNew slides keyType availablePositions =
        sortSlidesByKey slides keyType availablePositions true
        |> convertSortResultToDto