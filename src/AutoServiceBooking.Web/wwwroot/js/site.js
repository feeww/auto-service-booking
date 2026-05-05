const initializeSafely = (featureName, initialize) => {
    try {
        initialize();
    } catch (error) {
        console.error(`DriveFix UI error in ${featureName}`, error);
    }
};

const parseTimeToMinutes = (time) => {
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
};

const formatMinutesAsTime = (minutes) => {
    const hours = Math.floor(minutes / 60).toString().padStart(2, '0');
    const mins = (minutes % 60).toString().padStart(2, '0');
    return `${hours}:${mins}`;
};

const isSunday = (dateValue) => {
    if (!dateValue) {
        return false;
    }

    const [year, month, day] = dateValue.split('-').map(Number);
    return new Date(year, month - 1, day).getDay() === 0;
};

initializeSafely('navigation links', () => {
    const navLinks = Array.from(document.querySelectorAll('[data-nav-link]'));
    const sections = navLinks
        .map((link) => document.getElementById(link.dataset.navLink))
        .filter(Boolean);

    if (sections.length === 0) {
        return;
    }

    const setActiveLink = (sectionId) => {
        navLinks.forEach((link) => {
            link.classList.toggle('active', link.dataset.navLink === sectionId);
        });
    };

    const updateActiveLink = () => {
        const markerPosition = window.scrollY + 130;
        const pageBottom = window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 8;

        if (pageBottom) {
            setActiveLink(sections[sections.length - 1].id);
            return;
        }

        const currentSection = sections.reduce((current, section) => {
            return section.offsetTop <= markerPosition ? section : current;
        }, sections[0]);

        setActiveLink(currentSection.id);
    };

    window.addEventListener('scroll', updateActiveLink, { passive: true });
    window.addEventListener('hashchange', updateActiveLink);
    updateActiveLink();
});

initializeSafely('saved vehicle toggle', () => {
    const savedVehicleSelect = document.querySelector('[data-saved-vehicle-select]');
    const newVehicleFields = document.querySelector('[data-new-vehicle-fields]');
    const savedVehicleCards = Array.from(document.querySelectorAll('[data-saved-vehicle-card]'));

    if (!savedVehicleSelect || !newVehicleFields) {
        return;
    }

    const toggleVehicleFields = () => {
        const selectedVehicleId = savedVehicleSelect.value;
        const hasSelectedVehicle = selectedVehicleId !== '';

        newVehicleFields.classList.toggle('is-hidden', hasSelectedVehicle);

        savedVehicleCards.forEach((card) => {
            card.classList.toggle('is-hidden', card.dataset.savedVehicleCard !== selectedVehicleId);
        });
    };

    savedVehicleSelect.addEventListener('change', toggleVehicleFields);
    toggleVehicleFields();
});

initializeSafely('booking availability picker', () => {
    const scheduledAtInput = document.querySelector('[data-booking-date-input]');
    const dateSelect = document.querySelector('[data-booking-date-select]');
    const serviceSelect = document.querySelector('[data-service-select]');
    const slotGrid = document.querySelector('[data-time-slot-grid]');
    const slotHelper = document.querySelector('[data-time-slot-helper]');
    const warning = document.querySelector('[data-booking-date-warning]');
    const form = scheduledAtInput?.closest('form');
    const submitButton = form?.querySelector('button[type="submit"]');
    const workDaySettings = document.querySelector('[data-work-day-start]');

    if (!scheduledAtInput || !dateSelect || !serviceSelect || !slotGrid || !slotHelper || !warning || !form || !submitButton || !workDaySettings) {
        return;
    }

    const blockedDates = new Map(
        Array.from(document.querySelectorAll('[data-blocked-date]'))
            .map((element) => [element.dataset.blockedDate, element.dataset.blockedReason || 'запис на цю дату недоступний'])
    );

    const serviceDurations = new Map(
        Array.from(document.querySelectorAll('[data-service-duration]'))
            .map((element) => [element.dataset.serviceDuration, Number(element.dataset.durationMinutes || '60')])
    );

    const unavailableSlots = new Set(
        Array.from(document.querySelectorAll('[data-unavailable-date]'))
            .map((element) => `${element.dataset.unavailableService}|${element.dataset.unavailableDate}|${element.dataset.unavailableTime}`)
    );

    const workDayStart = Number(workDaySettings.dataset.workDayStart || '540');
    const workDayEnd = Number(workDaySettings.dataset.workDayEnd || '1140');
    const slotStep = Number(workDaySettings.dataset.slotStep || '30');

    const getSelectedDate = () => dateSelect.value || '';

    const getSelectedTime = () => {
        if (!scheduledAtInput.value || !scheduledAtInput.value.includes('T')) {
            return '';
        }

        return scheduledAtInput.value.split('T')[1].slice(0, 5);
    };

    const setSelectedTime = (time) => {
        scheduledAtInput.value = time ? `${getSelectedDate()}T${time}` : '';
    };

    const setWarning = (message) => {
        warning.textContent = message;
        warning.classList.toggle('is-hidden', message === '');
        dateSelect.classList.toggle('input-invalid', message !== '');
    };

    const isBusy = (serviceId, date, time) => {
        return unavailableSlots.has(`${serviceId}|${date}|${time}`);
    };

    const updateCalendarState = () => {
        const selectedDate = getSelectedDate();
        const selectedServiceId = serviceSelect.value;
        const durationMinutes = serviceDurations.get(selectedServiceId);
        const blockedReason = blockedDates.get(selectedDate);

        slotGrid.innerHTML = '';
        submitButton.disabled = true;

        if (isSunday(selectedDate)) {
            setSelectedTime('');
            setWarning('У неділю сервіс не працює. Оберіть інший день.');
            slotHelper.textContent = 'Неділя — вихідний день.';
            return;
        }

        if (blockedReason) {
            setSelectedTime('');
            setWarning(`На цю дату запис недоступний: ${blockedReason}. Оберіть інший день.`);
            slotHelper.textContent = 'Дата закрита адміністратором.';
            return;
        }

        setWarning('');

        if (!selectedServiceId || !durationMinutes) {
            setSelectedTime('');
            slotHelper.textContent = 'Оберіть послугу та дату, щоб побачити доступні години.';
            return;
        }

        const currentSelectedTime = getSelectedTime();
        let hasAvailableSlots = false;
        let selectedSlotStillAvailable = false;
        let firstAvailableTime = '';

        for (let start = workDayStart; start + durationMinutes <= workDayEnd; start += slotStep) {
            const time = formatMinutesAsTime(start);
            const button = document.createElement('button');
            const busy = isBusy(selectedServiceId, selectedDate, time);

            button.type = 'button';
            button.className = 'time-slot-button';
            button.textContent = time;

            if (busy) {
                button.disabled = true;
                button.title = 'Недоступно';
                button.classList.add('busy');
                button.setAttribute('aria-label', `${time}, недоступно`);
            } else {
                hasAvailableSlots = true;

                if (!firstAvailableTime) {
                    firstAvailableTime = time;
                }

                button.addEventListener('click', () => {
                    setSelectedTime(time);
                    updateCalendarState();
                });
            }

            if (currentSelectedTime === time && !busy) {
                button.classList.add('active');
                selectedSlotStillAvailable = true;
            }

            slotGrid.appendChild(button);
        }

        if (!hasAvailableSlots) {
            setSelectedTime('');
            slotHelper.textContent = 'На цю дату немає вільних годин для обраної послуги.';
            return;
        }

        if (selectedSlotStillAvailable) {
            setSelectedTime(currentSelectedTime);
        } else {
            setSelectedTime(firstAvailableTime);
            updateCalendarState();
            return;
        }

        slotHelper.textContent = 'Сині години — доступні. Сірі години недоступні.';
        submitButton.disabled = !scheduledAtInput.value;
    };

    dateSelect.addEventListener('change', updateCalendarState);
    dateSelect.addEventListener('input', updateCalendarState);
    serviceSelect.addEventListener('change', updateCalendarState);
    updateCalendarState();
});

initializeSafely('admin reschedule picker', () => {
    const forms = Array.from(document.querySelectorAll('[data-admin-reschedule-form]'));
    const workDaySettings = document.querySelector('[data-admin-work-day-start]');

    if (forms.length === 0 || !workDaySettings) {
        return;
    }

    const blockedDates = new Map(
        Array.from(document.querySelectorAll('[data-admin-blocked-date]'))
            .map((element) => [element.dataset.adminBlockedDate, element.dataset.adminBlockedReason || 'запис на цю дату недоступний'])
    );

    const occupiedIntervals = Array.from(document.querySelectorAll('[data-admin-occupied-date]')).map((element) => ({
        bookingId: Number(element.dataset.adminOccupiedBookingId || '0'),
        date: element.dataset.adminOccupiedDate,
        start: element.dataset.adminOccupiedStart,
        end: element.dataset.adminOccupiedEnd,
        label: element.dataset.adminOccupiedLabel || 'Зайнято'
    }));

    const workDayStart = Number(workDaySettings.dataset.adminWorkDayStart || '540');
    const workDayEnd = Number(workDaySettings.dataset.adminWorkDayEnd || '1140');
    const slotStep = Number(workDaySettings.dataset.adminSlotStep || '30');

    const getSelectedTime = (scheduledAtInput) => {
        if (!scheduledAtInput.value || !scheduledAtInput.value.includes('T')) {
            return '';
        }

        return scheduledAtInput.value.split('T')[1].slice(0, 5);
    };

    const getBusyReason = (date, startMinutes, endMinutes, ignoredBookingId) => {
        const busyInterval = occupiedIntervals.find((interval) => {
            if (interval.bookingId === ignoredBookingId || interval.date !== date) {
                return false;
            }

            const busyStart = parseTimeToMinutes(interval.start);
            const busyEnd = parseTimeToMinutes(interval.end);
            return startMinutes < busyEnd && endMinutes > busyStart;
        });

        return busyInterval ? `${busyInterval.label}: ${busyInterval.start}–${busyInterval.end}` : '';
    };

    const getBusyIntervalsText = (date, ignoredBookingId) => {
        const intervals = occupiedIntervals
            .filter((interval) => interval.date === date && interval.bookingId !== ignoredBookingId)
            .map((interval) => `${interval.label} ${interval.start}–${interval.end}`);

        return intervals.length === 0 ? '' : `Зайнято цього дня: ${intervals.join(', ')}.`;
    };

    forms.forEach((form) => {
        const bookingId = Number(form.dataset.bookingId || '0');
        const durationMinutes = Number(form.dataset.durationMinutes || '60');
        const dateInput = form.querySelector('[data-admin-reschedule-date]');
        const scheduledAtInput = form.querySelector('[data-admin-reschedule-value]');
        const slotGrid = form.querySelector('[data-admin-reschedule-slots]');
        const warning = form.querySelector('[data-admin-reschedule-warning]');
        const helper = form.querySelector('[data-admin-reschedule-helper]');
        const submitButton = form.querySelector('button[type="submit"]');

        if (!dateInput || !scheduledAtInput || !slotGrid || !warning || !helper || !submitButton) {
            return;
        }

        const setWarning = (message) => {
            warning.textContent = message;
            warning.classList.toggle('is-hidden', message === '');
            dateInput.classList.toggle('input-invalid', message !== '');
        };

        const setSelectedTime = (time) => {
            scheduledAtInput.value = time ? `${dateInput.value}T${time}` : '';
        };

        const updateSlots = () => {
            const selectedDate = dateInput.value;
            const selectedTime = getSelectedTime(scheduledAtInput);
            const blockedReason = blockedDates.get(selectedDate);

            slotGrid.innerHTML = '';
            submitButton.disabled = true;

            if (isSunday(selectedDate)) {
                setSelectedTime('');
                setWarning('У неділю сервіс не працює. Оберіть інший день.');
                helper.textContent = 'Неділя — вихідний день.';
                return;
            }

            if (blockedReason) {
                setSelectedTime('');
                setWarning(`На цю дату запис недоступний: ${blockedReason}.`);
                helper.textContent = 'Дата закрита адміністратором.';
                return;
            }

            setWarning('');

            let hasAvailableSlots = false;
            let selectedSlotStillAvailable = false;

            for (let start = workDayStart; start + durationMinutes <= workDayEnd; start += slotStep) {
                const end = start + durationMinutes;
                const time = formatMinutesAsTime(start);
                const busyReason = getBusyReason(selectedDate, start, end, bookingId);
                const button = document.createElement('button');

                button.type = 'button';
                button.className = 'time-slot-button';
                button.textContent = time;

                if (busyReason) {
                    button.disabled = true;
                    button.title = busyReason;
                    button.classList.add('busy');
                    button.setAttribute('aria-label', `${time}, зайнято: ${busyReason}`);
                } else {
                    hasAvailableSlots = true;
                    button.addEventListener('click', () => {
                        setSelectedTime(time);
                        updateSlots();
                    });
                }

                if (selectedTime === time && !busyReason) {
                    button.classList.add('active');
                    selectedSlotStillAvailable = true;
                }

                slotGrid.appendChild(button);
            }

            if (!hasAvailableSlots) {
                setSelectedTime('');
                helper.textContent = 'На цю дату немає вільних годин для цього запису.';
                return;
            }

            if (selectedSlotStillAvailable) {
                setSelectedTime(selectedTime);
            } else {
                setSelectedTime('');
            }

            const busyIntervalsText = getBusyIntervalsText(selectedDate, bookingId);
            helper.textContent = busyIntervalsText
                ? `${busyIntervalsText} Сірі години недоступні через перетин із зайнятим інтервалом.`
                : 'Сині години — доступні. Поточна заявка не блокує сама себе.';
            submitButton.disabled = !scheduledAtInput.value;
        };

        dateInput.addEventListener('change', updateSlots);
        dateInput.addEventListener('input', updateSlots);
        updateSlots();
    });

    document.querySelectorAll('[data-admin-reschedule-close]').forEach((button) => {
        button.addEventListener('click', () => {
            const details = button.closest('details');
            if (details) {
                details.open = false;
            }
        });
    });
});
