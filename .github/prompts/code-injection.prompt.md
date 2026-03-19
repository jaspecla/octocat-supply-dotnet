# Code Injection Vulnerability Demo

This prompt demonstrates how to introduce a Code injection vulnerability into the Delivery API for demonstration purposes. **WARNING: This creates an intentional security vulnerability that should NEVER be used in production code.**

## Overview

We are going to add a new logic block that is supposed to show how a command can be executed when updating the status of a delivery. This execution will be vulnerable to exploitation, allowing an attacker to inject arbitrary code into the application.

### New Branch

First create a new branch for the vulnerable implementation, called `code-injection-demo`. If this branch exists, create a new one with a different name like `code-injection-demo-1` or similar.

```bash
git checkout -b <branch_name>
```

### Create a Vulnerable Block


Update `src/OctocatSupply.Api/Controllers/DeliveriesController.cs`, replacing the existing `PUT /:id/status` action method. The vulnerable code uses `Process.Start()` from `System.Diagnostics` to run a user-supplied `notifyCommand` without any sanitization:

```csharp
// Update src/OctocatSupply.Api/Controllers/DeliveriesController.cs,
// replacing the existing PUT UpdateStatus action method

[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
{
    var delivery = await _repository.GetByIdAsync(id);

    if (delivery == null)
    {
        return NotFound("Delivery not found");
    }

    var updatedDelivery = await _repository.UpdateStatusAsync(id, request.Status);

    if (!string.IsNullOrEmpty(request.NotifyCommand))
    {
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {request.NotifyCommand}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            return Ok(new { delivery = updatedDelivery, commandOutput = output });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    return Ok(updatedDelivery);
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? NotifyCommand { get; set; }
}
```


**This vulnerability is for educational purposes only. Never deploy code with Code injection vulnerabilities to production.**

### Push and Create a PR

```bash
git add .
git commit -m "Add Code injection vulnerability for demo"
git push origin <branch_name>
```

Then, create a pull request (PR) from the `<branch_name>` branch to the main branch in the GitHub repository.
