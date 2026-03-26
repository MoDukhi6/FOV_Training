public interface ITrainingStage
{
    string StageName { get; }
    void Begin(TrainingSession session);
    void End();
    void Tick(); // per-frame if needed
    void OnUserResponse(); // generic response button
}
