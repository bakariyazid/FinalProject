(function () {
  const rules = [
    {
      test: (value) => value.length >= 8,
      message: "Must include at least 8 characters."
    },
    {
      test: (value) => /\d/.test(value),
      message: "Must include at least one number."
    },
    {
      test: (value) => /[^A-Za-z0-9]/.test(value),
      message: "Include a special character for better security."
    }
  ];

  function setToggleIcon(button, isVisible) {
    const icon = button.querySelector("i");

    button.setAttribute("aria-label", isVisible ? "Hide password" : "Show password");
    button.setAttribute("aria-pressed", isVisible.toString());

    if (!icon) {
      return;
    }

    icon.classList.toggle("fa-eye", isVisible);
    icon.classList.toggle("fa-eye-slash", !isVisible);
    icon.classList.toggle("bi-eye", isVisible);
    icon.classList.toggle("bi-eye-slash", !isVisible);
  }

  function updateStrength(input) {
    const field = input.closest("[data-password-strength]");
    const label = field.querySelector("[data-password-strength-label]");
    const labelWrap = label.closest(".password-strength-label");
    const requirements = field.querySelector("[data-password-requirements]");
    const value = input.value;
    const passedRules = rules.filter((rule) => rule.test(value));
    const missingRules = rules.filter((rule) => !rule.test(value));
    const strength = passedRules.length === rules.length
      ? "strong"
      : passedRules.length >= 1
        ? "medium"
        : "weak";

    field.classList.remove("is-weak", "is-medium", "is-strong");

    if (!value) {
      label.textContent = "";
      labelWrap.hidden = true;
      requirements.innerHTML = "";
      return;
    }

    labelWrap.hidden = false;
    field.classList.add(`is-${strength}`);
    label.textContent = strength;
    requirements.innerHTML = missingRules
      .map((rule) => `<li>${rule.message}</li>`)
      .join("");
  }

  document.querySelectorAll("[data-password-strength]").forEach((field) => {
    const input = field.querySelector("[data-password-strength-input]");
    const toggle = field.querySelector("[data-password-toggle]");

    if (!input) {
      return;
    }

    input.addEventListener("input", () => updateStrength(input));
    updateStrength(input);

    if (!toggle) {
      return;
    }

    toggle.addEventListener("click", () => {
      const isVisible = input.type === "password";
      input.type = isVisible ? "text" : "password";
      setToggleIcon(toggle, isVisible);
    });

    setToggleIcon(toggle, input.type !== "password");
  });
})();
