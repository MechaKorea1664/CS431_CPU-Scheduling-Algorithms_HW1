using System.Collections.Frozen;
using System.ComponentModel;
using System.Formats.Asn1;

void main()
{
    // Testing SJF
    List<Job> SJF_Job_list = [new Job(1,0, 3,1), new Job(2,2, 6,1), new Job(3,4, 4,1), new Job(4,6, 5,1), new Job(5,8, 2,1)];
    SJF(ref SJF_Job_list);

    // Testing RR with Quantum of 2
    List<Job> RR_Job_list = [new Job(1, 0, 2, 2), new Job(2, 1, 1, 1), new Job(3, 2, 8, 4), new Job(4, 3, 4, 2), new Job(5, 4, 5, 3)];
    RR(ref RR_Job_list, 2);

    // Testing SRT
    List<Job> SRT_Job_list = [new Job(1, 0, 3, 1), new Job(2, 2, 6, 1), new Job(3, 4, 4, 1), new Job(4, 6, 5, 1), new Job(5, 8, 2, 1)];
    SRT(ref SRT_Job_list);
}

main();

// Shortest Job First (SJF)
void SJF(ref List<Job> jobs)
{
    Console.WriteLine("Shortest Job First");
    int job_count = jobs.Count;
    int lowest_index = 0;
    int time = 0;
    double turnaround = 0;
    bool in_process = false;
    List<Job> wait_list = [];
    // Repeat until no processes remain.
    while (jobs.Count + wait_list.Count > 0)
    {
        // Add processes to the wait list with a matching arrival time.
        for (int i = 0; i < jobs.Count; i++)
        {
            if (jobs[i].Arrival_Time == time)
            {
                wait_list.Add(jobs[i]);
                jobs.RemoveAt(i);
                i--;
            }
        }
        // Find the process with least remaining burst time when no processes are in execution.
        if (!in_process)
        {
            int lowest = 999999999;
            lowest_index = 0;
            for (int i = 0; i < wait_list.Count; i++)
            {
                if (lowest > wait_list[i].CPU_Time)
                {
                    lowest = wait_list[i].CPU_Time;
                    lowest_index = i;
                }
            }
            in_process = true;
        }
        // Decrement the burst time by 1 for the chosen process from above if statement.
        wait_list[lowest_index].Decrement_CPU_Time();
        // When process is terminated, output the result, calculate turnaround time, then prepare to
        // choose the next process with least remaining burst time.
        if (wait_list[lowest_index].CPU_Time == 0)
        {
            double process_turnaround = wait_list[lowest_index].Calculate_Turnaround_Time(time+1);
            Console.WriteLine("P" + wait_list[lowest_index].Process_Number + " complete, with turnaround: " + process_turnaround);
            turnaround += process_turnaround;
            wait_list.RemoveAt(lowest_index);
            in_process = false;
        }
        // Count the time.
        time++;
    }
    Console.Write("Average Turnaround Time: " + turnaround/job_count + " (Cycles: " + time + ")");
}

// Round Robin (RR)
void RR(ref List<Job> jobs, int quantum)
{
    Console.WriteLine("\n\nRound Robin w/ Priority");
    int jobs_count = jobs.Count;
    int time = 0;
    int spent_time = 0; // time spent running a process
    LinkedList<Job> wait_LL = new LinkedList<Job>();
    double turnaround = 0;
    string output = "";
    Job target = null;
    // Find the first job
    foreach (Job job in jobs)
    {
        if (job.Arrival_Time == time)
        {
            target = job;
            break;
        }
        time++;
    }
    if (target == null)
    {
        return;
    }
    while (jobs.Count+wait_LL.Count > 0)
    {
        // Change the process being processed every given quantum
        // Only change the target when the remaining burst time is 0 or
        // quantum has been reached.
        if (target.CPU_Time == 0)
        {
            output += $"P{target.Process_Number} > ";
            double process_turnaround = target.Calculate_Turnaround_Time(time);
            turnaround += process_turnaround;
            Console.WriteLine($"P{target.Process_Number} complete, with turnaround: {process_turnaround}");
            wait_LL.Remove(target);
            spent_time = 0;
        }
        if (spent_time == quantum)
        {
            output += $"P{target.Process_Number} > ";
            
        }
        // Arrange incoming processes by priority. If new process priority
        // is greater than the first item on the "queue", set the new process
        // as the head.
        for (int i = 0; i < jobs.Count; i++)
        {
            if (jobs[i].Arrival_Time <= time)
            {
                if (wait_LL.Count == 0)
                {
                    wait_LL.AddFirst(jobs[i]);
                    jobs.RemoveAt(i);
                    i--;
                    continue;
                }
                if (jobs[i].Priority < wait_LL.First().Priority) { wait_LL.AddFirst(jobs[i]); }
                else { wait_LL.AddLast(jobs[i]); }
                jobs.RemoveAt(i);
                i--;
            }
        }
        // Set target for decrement
        if (wait_LL.Count > 0 && (target.CPU_Time == 0 || spent_time == quantum))
        {
            target = wait_LL.First();
            spent_time = 0;
        }
        // Run process
        target.Decrement_CPU_Time();
        
        // keep track of time
        spent_time++;
        time++;
    }
    // debug
    // Console.WriteLine(output);
    Console.WriteLine($"Average Turnaround Time: {turnaround / jobs_count} (Cycles: {time})");
}

// Shortest Remaining Time (SRT)
void SRT(ref List<Job> jobs)
{
    Console.WriteLine("\nShortest Remaining Time");
    int time = 0;
    double turnaround = 0;
    int jobs_count = jobs.Count;
    List<Job> wait_list = [];
    while (jobs.Count + wait_list.Count > 0)
    {
        // look through every item in the list to find the job that should
        // arrive at current value of time. (just in case list is unsorted)
        for (int i = 0; i < jobs.Count; i++)
        {
            if (jobs[i].Arrival_Time == time)
            {
                wait_list.Add(jobs[i]);
                jobs.Remove(jobs[i]);
                i--;
            }
        }
        // Sort the list by the remaining CPU time.
        wait_list = wait_list.OrderBy(Job => Job.CPU_Time).ToList();
        // Now decrement the CPU time for the first (shortest) process.
        wait_list[0].Decrement_CPU_Time();
        // Calculate turnaround and remove from waitlist after full execution.
        if (wait_list[0].CPU_Time == 0)
        {
            double process_turnaround = wait_list[0].Calculate_Turnaround_Time(time+1);
            turnaround += process_turnaround;
            Console.WriteLine($"P{wait_list[0].Process_Number} Complete, with turnaround {process_turnaround}");
            wait_list.RemoveAt(0);
        }
        // keeping time
        time++;
    }
    // Output the average turnaround.
    Console.WriteLine($"Average Turnaround Time: {turnaround / jobs_count} (Cycles: {time})");
}

public class Job
{
    public Job(int process_number, int arrival_time, int cpu_time, int priority)
    {
        Process_Number = process_number;
        Arrival_Time = arrival_time;
        CPU_Time = cpu_time;
        Priority = priority;
    }
    public int Process_Number { get; }
    public int Arrival_Time { get; }
    public int CPU_Time { get; set; }
    public int Priority { get; }
    public void Decrement_CPU_Time() { CPU_Time--; }
    public int Calculate_Turnaround_Time(int completion_time) { return completion_time - Arrival_Time; }
    public override string ToString()
    {
        return $"P{Process_Number} AT: {Arrival_Time} CT: {CPU_Time} P: {Priority}";
    }
}