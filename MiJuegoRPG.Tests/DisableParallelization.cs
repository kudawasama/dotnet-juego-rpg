using Xunit;

// Desactiva la paralelización de pruebas para evitar interferencias con RandomService (singleton compartido)
[assembly: CollectionBehavior(DisableTestParallelization = true)]
