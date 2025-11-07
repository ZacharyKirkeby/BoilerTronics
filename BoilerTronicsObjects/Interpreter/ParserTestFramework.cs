using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Parsing;

namespace ParsingTests;

public class ParserTestFramework
{
	private int _passedTests = 0;
	private int _failedTests = 0;
	private List<string> _failureMessages = new();

	public void RunAllTests()
    {
        // fat so you can read it in our god forsaken debug terminal
		GD.Print("========================================");
		GD.Print("Starting Parser Test Suite");
		GD.Print("========================================\n");

		// Validation Tests
		TestValidMovCommands();
		TestInvalidMovCommands();
		TestValidRotCommands();
		TestInvalidRotCommands();
		TestValidClawCommands();
		TestInvalidClawCommands();
		TestValidWrtCommands();
		TestInvalidWrtCommands();
		TestValidArithCommands();
		TestInvalidArithCommands();
		TestValidJumpCommands();
		TestInvalidJumpCommands();
		TestValidWaitCommands();
		TestInvalidWaitCommands();
		TestLabelHandling();
		TestCompletePrograms();
		TestEdgeCases();

		// Print Summary
		GD.Print("\n========================================");
		GD.Print("Test Summary");
		GD.Print("========================================");
		GD.Print($"Passed: {_passedTests}");
		GD.Print($"Failed: {_failedTests}");
		GD.Print($"Total:  {_passedTests + _failedTests}");
		
		if (_failedTests > 0)
		{
			GD.Print("\n========================================");
			GD.Print("Failed Test Details");
			GD.Print("========================================");
			foreach (var msg in _failureMessages)
			{
				GD.Print(msg);
			}
		}
		
		GD.Print("========================================\n");
	}

	

	private void AssertTrue(bool condition, string testName)
	{
		if (condition)
		{
			_passedTests++;
			GD.Print($"✓ PASS: {testName}");
		}
		else
		{
			_failedTests++;
			string msg = $"✗ FAIL: {testName}";
			GD.PrintErr(msg);
			_failureMessages.Add(msg);
		}
	}

	private void AssertValidProgram(string program, string testName)
	{
		var errors = ProgramValidator.ValidateProgram(program);
		AssertTrue(errors.Count == 0, testName);
	}

	private void AssertInvalidProgram(string program, string testName, string expectedError = null)
	{
		var errors = ProgramValidator.ValidateProgram(program);
		bool hasError = errors.Count > 0;
		
		if (expectedError != null && hasError)
		{
			bool foundExpected = errors.Any(e => e.error.Contains(expectedError));
			AssertTrue(foundExpected, $"{testName} (expected error: {expectedError})");
		}
		else
		{
			AssertTrue(hasError, testName);
		}
	}

	private void TestValidMovCommands()
	{
		AssertValidProgram("mov l", "Valid mov left");
		AssertValidProgram("mov r", "Valid mov right");
		AssertValidProgram("mov u", "Valid mov up");
		AssertValidProgram("mov d", "Valid mov down");
		AssertValidProgram("  mov l  ", "whitespace");
		AssertValidProgram("MOV L", "Valid mov uppercase");
	}


    private void TestInvalidMovCommands()
    {
        AssertInvalidProgram("mov", "Invalid mov - missing argument", "missing argument");
        AssertInvalidProgram("mov x", "Invalid mov - bad direction", "Invalid move argument");
        AssertInvalidProgram("mov l r", "Invalid mov - too many args");
        AssertInvalidProgram("mov 1", "Invalid mov - numeric arg");
    }

	private void TestValidRotCommands()
	{
		AssertValidProgram("rot l", "Valid rot left");
		AssertValidProgram("rot r", "Valid rot right");
		AssertValidProgram("  rot l  ", "Valid rot with whitespace");
		AssertValidProgram("ROT R", "Valid rot uppercase");
	}

	private void TestInvalidRotCommands()
	{
		AssertInvalidProgram("rot", "Invalid rot - missing argument", "missing argument");
		AssertInvalidProgram("rot x", "Invalid rot - bad direction", "Invalid rotate argument");
		AssertInvalidProgram("rot u", "Invalid rot - invalid direction");
		AssertInvalidProgram("rot l r", "Invalid rot - too many args");
	}

	private void TestValidClawCommands()
	{
		AssertValidProgram("grb", "Valid grb");
		AssertValidProgram("drp", "Valid drp");
		AssertValidProgram("swt", "Valid swt");
		AssertValidProgram("  grb  ", "Valid grb with whitespace");
		AssertValidProgram("GRB", "Valid grb uppercase");
	}

	private void TestInvalidClawCommands()
	{
		AssertInvalidProgram("grb x", "Invalid grb - with argument", "takes no arguments");
		AssertInvalidProgram("drp 1", "Invalid drp - with argument", "takes no arguments");
		AssertInvalidProgram("swt r0", "Invalid swt - with argument", "takes no arguments");
	}

	private void TestValidWrtCommands()
	{
		AssertValidProgram("wrt r0 5", "Valid wrt r0 literal");
		AssertValidProgram("wrt r1 -10", "Valid wrt r1 negative");
		AssertValidProgram("wrt r2 0", "Valid wrt r2 zero");
		AssertValidProgram("wrt cmp 100", "Valid wrt cmp");
		AssertValidProgram("wrt r0 r1", "Valid wrt register to register");
		AssertValidProgram("wrt r1 cmp", "Valid wrt cmp to register");
		AssertValidProgram("  wrt r0 5  ", "Valid wrt with whitespace");
	}

	private void TestInvalidWrtCommands()
	{
		AssertInvalidProgram("wrt", "Invalid wrt - no args", "missing");
		AssertInvalidProgram("wrt r0", "Invalid wrt - missing value", "missing value");
		AssertInvalidProgram("wrt r3 5", "Invalid wrt - bad register");
		AssertInvalidProgram("wrt x 5", "Invalid wrt - invalid register");
		AssertInvalidProgram("wrt r0 abc", "Invalid wrt - non-numeric value");
	}

	private void TestValidArithCommands()
	{
		AssertValidProgram("add r0 r1", "Valid add registers");
		AssertValidProgram("sub r0 5", "Valid sub register literal");
		AssertValidProgram("mul 3 r2", "Valid mul literal register");
		AssertValidProgram("div r1 2", "Valid div");
		AssertValidProgram("cmp r0 r1", "Valid cmp");
		AssertValidProgram("add -5 r0", "Valid add negative");
		AssertValidProgram("cmp cmp r0", "Valid cmp with cmp register");
		AssertValidProgram("  add r0 r1  ", "Valid add with whitespace");
	}

	private void TestInvalidArithCommands()
	{
		AssertInvalidProgram("add", "Invalid add - no operands", "Missing operands");
		AssertInvalidProgram("add r0", "Invalid add - one operand", "Missing second operand");
		AssertInvalidProgram("sub r3 r0", "Invalid sub - bad register");
		AssertInvalidProgram("mul r0 x", "Invalid mul - bad operand");
		AssertInvalidProgram("div", "Invalid div - no operands");
		AssertInvalidProgram("cmp r0", "Invalid cmp - one operand");
	}

	private void TestValidJumpCommands()
	{
		string program = @"
			start:
			mov l
			jmp start
		";
		AssertValidProgram(program, "Valid jmp to label");

		program = @"
			loop:
			cmp r0 5
			jeq end
			add r0 1
			jmp loop
			end:
		";
		AssertValidProgram(program, "Valid jeq");

		program = @"
			loop:
			cmp r0 5
			jne loop
		";
		AssertValidProgram(program, "Valid jne");

		program = @"
			loop:
			cmp r0 5
			jgt end
			jmp loop
			end:
		";
		AssertValidProgram(program, "Valid jgt");

		program = @"
			loop:
			cmp r0 5
			jlt loop
		";
		AssertValidProgram(program, "Valid jlt");

		program = @"
			loop:
			cmp r0 5
			jge end
			jmp loop
			end:
		";
		AssertValidProgram(program, "Valid jge");

		program = @"
			loop:
			cmp r0 5
			jle loop
		";
		AssertValidProgram(program, "Valid jle");
	}

	private void TestInvalidJumpCommands()
	{
		AssertInvalidProgram("jmp", "Invalid jmp - no label", "missing label");
		AssertInvalidProgram("jmp nonexistent", "Invalid jmp - undefined label", "Undefined label");
		AssertInvalidProgram("jeq", "Invalid jeq - no label", "missing label");
		AssertInvalidProgram("jne", "Invalid jne - no label");
		AssertInvalidProgram("jgt nonexistent", "Invalid jgt - undefined label", "Undefined label");
	}

	private void TestValidWaitCommands()
	{
		AssertValidProgram("wait", "Valid wait");
		AssertValidProgram("  wait  ", "Valid wait with whitespace");
		AssertValidProgram("WAIT", "Valid wait uppercase");
	}

	private void TestInvalidWaitCommands()
	{
		AssertInvalidProgram("wait 1", "Invalid wait - with argument", "takes no arguments");
		AssertInvalidProgram("wait r0", "Invalid wait - with register");
	}

	private void TestLabelHandling()
	{
		string program = @"
			start:
			mov l
			end:
		";
		AssertValidProgram(program, "Valid multiple labels");

		program = @"
			start:
			start:
		";
		AssertInvalidProgram(program, "Invalid - duplicate labels", "Duplicate label");

		program = @"
			loop:
			mov l
			jmp loop
		";
		AssertValidProgram(program, "Valid label reference");

		// Test label case insensitivity
		program = @"
			START:
			jmp start
		";
		AssertValidProgram(program, "Valid label - case insensitive");
	}

	private void TestCompletePrograms()
	{
		// Simple loop
		string program = @"
			wrt r0 0
			loop:
			mov l
			add r0 1
			cmp r0 5
			jlt loop
		";
		AssertValidProgram(program, "Complete program - simple loop");

		// Complex program with multiple jumps
		program = @"
			wrt r0 0
			wrt r1 10
			start:
			cmp r0 r1
			jeq done
			mov r
			add r0 1
			jmp start
			done:
			wait
		";
		AssertValidProgram(program, "Complete program - conditional loop");

		// Program with arithmetic
		program = @"
			wrt r0 5
			wrt r1 3
			add r0 r1
			mul r0 2
			sub r0 1
			div r0 3
		";
		AssertValidProgram(program, "Complete program - arithmetic operations");

		// Program with claw operations
		program = @"
			mov l
			grb
			mov r
			mov r
			drp
		";
		AssertValidProgram(program, "Complete program - claw operations");

		// Mixed program
		program = @"
			wrt r0 0
			loop:
			mov l
			grb
			mov r
			drp
			add r0 1
			cmp r0 3
			jlt loop
			wait
		";
		AssertValidProgram(program, "Complete program - mixed operations");
	}

	private void TestEdgeCases()
	{
		// Empty program
		AssertTrue(ProgramValidator.IsValid(""), "Edge case - empty program");
		
		// Only whitespace
		AssertTrue(ProgramValidator.IsValid("   \n  \n  "), "Edge case - only whitespace");
		
		// Only comments (labels without instructions)
		string program = @"
			start:
			end:
		";
		AssertValidProgram(program, "Edge case - only labels");

		// Very long program
		var longProgram = "";
		for (int i = 0; i < 100; i++)
		{
			longProgram += "mov l\n";
		}
		AssertValidProgram(longProgram, "Edge case - very long program");

		// Mixed case
		program = @"
			START:
			MoV L
			WrT R0 5
			JmP start
		";
		AssertValidProgram(program, "Edge case - mixed case");

		// Negative numbers
		program = @"
			wrt r0 -100
			add r0 -5
			cmp -10 r0
		";
		AssertValidProgram(program, "Edge case - negative numbers");

		// Register to register operations
		program = @"
			wrt r0 5
			wrt r1 r0
			wrt r2 r1
			wrt cmp r2
		";
		AssertValidProgram(program, "Edge case - register to register writes");

		// All conditional jumps
		program = @"
			start:
			cmp r0 5
			jeq start
			jne start
			jgt start
			jlt start
			jge start
			jle start
		";
		AssertValidProgram(program, "Edge case - all conditional jumps");

		// Test GetInstructionCount
		program = @"
			start:
			mov l
			mov r
			end:
			wait
		";
		var result = ProgramValidator.PreprocessProgram(program);
		AssertTrue(result.validLines.Count == 3, "Edge case - instruction count excludes labels");

		// Test label lookup
		AssertTrue(ProgramValidator.HasLabel(program, "start"), "Edge case - has label 'start'");
		AssertTrue(ProgramValidator.HasLabel(program, "end"), "Edge case - has label 'end'");
		AssertTrue(!ProgramValidator.HasLabel(program, "missing"), "Edge case - doesn't have label 'missing'");
	}

}