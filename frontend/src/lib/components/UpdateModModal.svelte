<script lang="ts">
	import { isAdmin } from '$lib/stores/auth';
	import { updateMod } from '$lib/api/client';
	import { loadMods } from '$lib/stores/mods';
	import type { Mod } from '$lib/types';

	let { mod, onClose }: { mod: Mod; onClose: () => void } = $props();

	let isLoading = $state(false);
	let error = $state('');
	let success = $state(false);

	async function handleConfirm() {
		error = '';
		isLoading = true;

		const token = isAdmin.getToken();
		if (!token) {
			error = 'Session expired. Please log in again.';
			isLoading = false;
			return;
		}

		try {
			const result = await updateMod(mod.fileName, token);

			if (result.success) {
				success = true;
				// Refresh the mods list
				await loadMods();
				// Auto-close after success
				setTimeout(() => {
					onClose();
				}, 1500);
			} else {
				error = result.message || 'Update failed';
			}
		} catch (e) {
			if (e instanceof Error) {
				error = e.message;
			} else {
				error = 'An unexpected error occurred';
			}
		} finally {
			isLoading = false;
		}
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape' && !isLoading) {
			onClose();
		}
	}

	function handleBackdropClick(e: MouseEvent) {
		if (e.target === e.currentTarget && !isLoading) {
			onClose();
		}
	}
</script>

<svelte:window onkeydown={handleKeydown} />

<div class="modal-backdrop" onclick={handleBackdropClick} role="presentation">
	<div class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title">
		<h2 id="modal-title">Update Mod</h2>

		{#if success}
			<div class="success-message">
				<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
					<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
					<polyline points="22 4 12 14.01 9 11.01"></polyline>
				</svg>
				<span>Update successful!</span>
			</div>
		{:else}
			<div class="mod-info">
				<p class="mod-name">{mod.name}</p>
				<div class="version-transition">
					<span class="version current">{mod.version}</span>
					<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
						<line x1="5" y1="12" x2="19" y2="12"></line>
						<polyline points="12 5 19 12 12 19"></polyline>
					</svg>
					<span class="version new">{mod.latestCurseForgeVersion}</span>
				</div>
			</div>

			{#if error}
				<p class="error">{error}</p>
			{/if}

			<div class="actions">
				<button type="button" class="cancel-btn" onclick={onClose} disabled={isLoading}>
					Cancel
				</button>
				<button type="button" class="confirm-btn" onclick={handleConfirm} disabled={isLoading}>
					{#if isLoading}
						<span class="spinner"></span>
						Updating...
					{:else}
						Confirm Update
					{/if}
				</button>
			</div>
		{/if}
	</div>
</div>

<style>
	.modal-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(0, 0, 0, 0.5);
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 1000;
	}

	.modal {
		background: var(--bg-secondary);
		border: 1px solid var(--border-color);
		border-radius: 12px;
		padding: 24px;
		width: 100%;
		max-width: 400px;
		box-shadow: 0 4px 24px rgba(0, 0, 0, 0.2);
	}

	h2 {
		margin: 0 0 20px 0;
		font-size: 20px;
		font-weight: 600;
		color: var(--text-primary);
	}

	.mod-info {
		margin-bottom: 20px;
	}

	.mod-name {
		font-size: 16px;
		font-weight: 600;
		color: var(--text-primary);
		margin: 0 0 12px 0;
	}

	.version-transition {
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 12px;
		background: var(--bg-primary);
		border-radius: 8px;
	}

	.version-transition svg {
		color: var(--text-secondary);
	}

	.version {
		font-family: monospace;
		font-size: 14px;
		padding: 4px 8px;
		border-radius: 4px;
	}

	.version.current {
		background: #374151;
		color: var(--text-secondary);
	}

	.version.new {
		background: #16a34a;
		color: white;
	}

	.error {
		color: #ef4444;
		font-size: 14px;
		margin: 0 0 16px 0;
		padding: 10px;
		background: rgba(239, 68, 68, 0.1);
		border-radius: 6px;
	}

	.success-message {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 12px;
		padding: 24px;
		color: #16a34a;
		font-size: 16px;
		font-weight: 500;
	}

	.actions {
		display: flex;
		gap: 12px;
		justify-content: flex-end;
	}

	button {
		padding: 10px 20px;
		font-size: 14px;
		font-weight: 500;
		border-radius: 8px;
		cursor: pointer;
		transition: all 0.2s ease;
		display: flex;
		align-items: center;
		gap: 8px;
	}

	button:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.cancel-btn {
		background: none;
		border: 1px solid var(--border-color);
		color: var(--text-secondary);
	}

	.cancel-btn:hover:not(:disabled) {
		border-color: var(--text-secondary);
		color: var(--text-primary);
	}

	.confirm-btn {
		background: #16a34a;
		border: none;
		color: white;
	}

	.confirm-btn:hover:not(:disabled) {
		background: #15803d;
	}

	.spinner {
		width: 14px;
		height: 14px;
		border: 2px solid rgba(255, 255, 255, 0.3);
		border-top-color: white;
		border-radius: 50%;
		animation: spin 1s linear infinite;
	}

	@keyframes spin {
		to { transform: rotate(360deg); }
	}
</style>
