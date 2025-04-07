## Run

To start the solution, execute the following command from the root of your solution:

```bash
docker-compose up --build
```

This version is clearer and follows a more standard format for startup instructions. It highlights the necessary steps and clarifies that the `data` directory is mounted to the root project directory.


## 🧩 Design SensorPOC

**SensorPOC** is responsible for generating and persisting synthetic sensor data at a consistent interval of **1 Hz**.

To maintain a clean separation of concerns and enable scalability, the component is split into two background services:

---

- **`DataGeneratorWorker`**  
  Generates synthetic sensor data every second using an infinite loop combined with a `PeriodicTimer`.  
  Each data point is pushed into a **FIFO channel** for downstream processing.

- **`FileWriterWorker`**  
  Continuously reads sensor data from the channel and writes it to a CSV file.  
  After **100 entries**, it closes the current file and starts a new one with a rolled-over filename.

  Once a file is completed, it signals readiness by creating a corresponding `.done` marker file (e.g., `SensorPOC_20250303_101.done`).

---

### 🔍 Design Rationale & Choices

- **Decoupled Workflows**  
  File writing and data generation operate independently, promoting scalability by extending the pipeline and enhancing the testability of separate stages.

- **Consistent Intervals**  
  `PeriodicTimer.WaitForNextTickAsync()` ensures precise timing and prevents drift, unlike using `Thread.Sleep()` or `Task.Delay()`.

- **Graceful Shutdown using CancellationToken**  
  Propagate `CancellationToken` to the service loops or async functions to gracefully shut down, e.g., stop the file write loop and add a `.done` file to any file that was started before shutdown.

---

### 🔧 Areas for Improvement or Future Enhancements

- **Refactor FileWriter to Depend on `ChannelReader<ISensorModel>`**  
  Refactor the `FileWriter` to use `ChannelReader<ISensorModel>`, with functions like `GetCsvHeader()`, `GetCsvRow()`, and a `SensorName` property. This removes the dependency on a specific sensor type (e.g., lat/lon sensor generator) and allows for greater flexibility with different sensor models.

- **Handle restart**  
  Add a unique identifier, such as the program start time, to the file name to prevent duplicates after a restart.

- **Add Logging and Tests for Potential Issues**  
  Add logging to capture errors related to file or directory creation failures.

---

## 🧩 Design IngesterPOC

**IngesterPOC** is responsible for reading sensor data and logging a summary.

The component is split into two background services:

---
- **`FileDetectionWorker`**  
  Watches the output directory for new `.done` marker files, indicating that a corresponding CSV file is ready for ingestion.  
  Once detected, the file path is pushed into a channel for further processing. This allows decoupling detection logic from file processing.

- **`FileProcessingWorker`**  
  Consumes file paths from the channel and reads the associated CSV files.  
  It parses the data, computes a summary (e.g., row count, time range, value stats), and logs the results.

---

### 🔍 Design Rationale & Choices

#### Readiness Strategies

When detecting if a file is ready for processing, several strategies can be used. Given our knowledge of the sensor's behavior — such as the write interval, number of lines, and file naming pattern — we selected a simple and robust approach:

- **Chosen Strategy: `.done` File Signal**  
  Once writing is complete, the sensor creates a `.done` file (e.g., `SensorPOC_20250401_101.done`).  
  This approach is **explicit**, **reliable**, and easy to implement since we control both the producer and consumer.

Considerations:
- **File Row Count**  
  Monitor line count. IO intensive;

- **File Locking**  
  Check if the file is locked or in use (requires OS-level support and coordination).

- **Inactivity Timeout**  
  Assume readiness after no writes occur for a specific duration. Not always reliable.

- **Rollover Processing**  
  Detect a new file creation and process the previous one using predictable file naming. Need custom logic for last file. 

#### Working with csv

- **Read Files Line by Line**  
  Process the file line by line instead of loading all lines into memory with `ReadAllLinesAsync` to reduce memory overhead, especially for larger files.

- **Use CsvHelper**  
  Use `CsvHelper` for efficient CSV reading and writing, simplifying header management and row mapping, making the code cleaner and more maintainable.

- **Support Extended Key-Value Pairs**
  Extend support to handle dynamic key-value pairs (instead of just `key1, value1, key2, value2`) by using a dictionary or another flexible data structure.



